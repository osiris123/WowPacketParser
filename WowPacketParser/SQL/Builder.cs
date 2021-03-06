﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL.Builders;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

namespace WowPacketParser.SQL
{
    public static class Builder
    {
        public static void DumpSQL(string prefix, string fileName, string header)
        {
            var startTime = DateTime.Now;

            var units = Storage.Objects.IsEmpty()
                ? new Dictionary<WowGuid, Unit>()                                                               // empty dict if there are no objects
                : Storage.Objects.Where(
                    obj =>
                        obj.Value.Item1.Type == ObjectType.Unit && obj.Key.GetHighType() != HighGuidType.Pet && // remove pets
                        !obj.Value.Item1.IsTemporarySpawn())                                                    // remove temporary spawns
                    .OrderBy(pair => pair.Value.Item2)                                                          // order by spawn time
                    .ToDictionary(obj => obj.Key, obj => obj.Value.Item1 as Unit);

            var gameObjects = Storage.Objects.IsEmpty()
                ? new Dictionary<WowGuid, GameObject>()                                                         // empty dict if there are no objects
                : Storage.Objects.Where(obj => obj.Value.Item1.Type == ObjectType.GameObject)
                    .OrderBy(pair => pair.Value.Item2)                                                          // order by spawn time
                    .ToDictionary(obj => obj.Key, obj => obj.Value.Item1 as GameObject);

            foreach (var obj in Storage.Objects)
                obj.Value.Item1.LoadValuesFromUpdateFields();

            using (var store = new SQLFile(fileName))
            {
                var builderMethods = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => type.GetCustomAttributes(typeof (BuilderClassAttribute), true).Length > 0)
                    .SelectMany(x => x.GetMethods())
                    .Where(y => y.GetCustomAttributes().OfType<BuilderMethodAttribute>().Any())
                    .ToList();

                var i = 0;
                foreach (var method in builderMethods)
                {
                    var attr = method.GetCustomAttribute<BuilderMethodAttribute>();

                    if (attr.CheckVersionMismatch)
                    {
                        if (!((ClientVersion.Expansion == ClientType.WrathOfTheLichKing &&
                             Settings.TargetedDatabase == TargetedDatabase.WrathOfTheLichKing)
                            ||
                            (ClientVersion.Expansion == ClientType.Cataclysm &&
                             Settings.TargetedDatabase == TargetedDatabase.Cataclysm)
                            ||
                            (ClientVersion.Expansion == ClientType.WarlordsOfDraenor &&
                             Settings.TargetedDatabase == TargetedDatabase.WarlordsOfDraenor)
                            ||
                            (ClientVersion.Expansion == ClientType.Legion &&
                             Settings.TargetedDatabase == TargetedDatabase.Legion)))
                        {
                            Trace.WriteLine($"Error: Couldn't generate SQL output of {method.Name} since the targeted database and the sniff version don't match.");
                            continue;
                        }
                    }

                    var parameters = new List<object>();
                    if (attr.Units)
                        parameters.Add(units);

                    if (attr.Gameobjects)
                        parameters.Add(gameObjects);

                    Trace.WriteLine($"{++i}/{builderMethods.Count} - Write {method.Name}");
                    try
                    {
                        store.WriteData(method.Invoke(null, parameters.ToArray()).ToString());
                    }
                    catch (TargetInvocationException e)
                    {
                        ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                    }
                }

                Trace.WriteLine(store.WriteToFile(header)
                    ? $"{prefix}: Saved file to '{fileName}'"
                    : "No SQL files created -- empty.");
                var endTime = DateTime.Now;
                var span = endTime.Subtract(startTime);
                Trace.WriteLine($"Finished SQL file in {span.ToFormattedString()}.");
            }
        }
    }
}
