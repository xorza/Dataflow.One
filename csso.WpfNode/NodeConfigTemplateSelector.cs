using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using csso.Common;
using csso.NodeCore;

namespace csso.WpfNode; 

public class NodeConfigTemplateSelector : DataTemplateSelector {
    public DataTemplate? Int32DataTemplate { get; set; }

    public Dictionary<Type, DataTemplate> DataTemplates { get; } = new();

    public override DataTemplate? SelectTemplate(object item, DependencyObject container) {
        if (item is FunctionConfig config) {
            if ((config.Type == typeof(Int32)
                ||config.Type == typeof(UInt32))
                && Int32DataTemplate != null)
                return Int32DataTemplate;
            
            DataTemplates.TryGetValue(config.Type, out var result);
            return result;
        }

        Debug.Assert.False();
        return null;
    }
}