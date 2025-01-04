using System;
using System.Collections.Generic;
using System.Net;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace RelicBuddy.Helpers.Strings;

public class StringsDict
{
    public Dictionary<string, RBStringTemplateRenderer?> dict { get; private set; } = [];
    public static StringsDict? _instance = null;
    public static StringsDict Instance => _instance ??= new StringsDict();
    public void Init(Plugin plugin)
    {
        plugin.RelicData.ForEach(expansion =>
        {
            expansion.Steps.ForEach(step =>
            {
                if (step.Hint != null)
                {
                    if (dict.ContainsKey(step.Hint))
                        return;
                    var template = new RBStringTemplateRenderer();
                    template.ProcessTemplate(step.Hint);
                    dict.Add(step.Hint, template);
                }

                if (step.Hints != null)
                {
                    foreach (var hint in step.Hints)
                    {
                        if (dict.ContainsKey(hint))
                            return;
                        var template = new RBStringTemplateRenderer();
                        template.ProcessTemplate(hint);
                        dict.Add(hint, template);
                    }
                }
                step.Requirements?.Item.ForEach(i =>
                {
                    foreach (var hint in i.Hints)
                    {
                        if (dict.ContainsKey(hint))
                            return;
                        var template = new RBStringTemplateRenderer();
                        template.ProcessTemplate(hint);
                        dict.Add(hint, template);
                    }
                });
            });
        });
    }

    public RBStringTemplateRenderer? GetTemplateRenderer(string key)
    {
        if(dict.Count == 0) {
            throw new InvalidOperationException("StringsDict is not initialized");
        }
        dict.TryGetValue(key, out RBStringTemplateRenderer? value);
        if (value is null)
        {
            Plugin.PluginLog.Warning($"Tried to get unknown template '{key}'");
            return null;
        }

        return value;
    }
}
