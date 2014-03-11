﻿using SharpDox.Local;
using SharpDox.Sdk.Config;
using SharpDox.Sdk.Config.Attributes;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using SharpDox.Sdk.Config.Lists;
using SharpDox.Sdk.Exporter;
using SharpDox.Sdk.Build;

namespace SharpDox.GUI.Controls.ConfigGrid
{
    public partial class ConfigSectionControl : UserControl
    {
        public static readonly DependencyProperty SectionHeaderProperty = DependencyProperty.Register("SectionHeader", typeof(string), typeof(ConfigSectionControl));
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ConfigSectionControl));

        private readonly LocalController _localController;
        private readonly IExporter[] _allExporters;
        private readonly IBuildController _buildController;
        private readonly ICoreConfigSection _coreConfigSection;

        public ConfigSectionControl(LocalController localController, ICoreConfigSection coreConfigSection, IExporter[] allExporters, IBuildController buildController)
        {
            _localController = localController;
            _coreConfigSection = coreConfigSection;
            _allExporters = allExporters;
            _buildController = buildController;

            DataContext = this;
            InitializeComponent();
        }

        private void ConfigSectionChanged(IConfigSection configSection)
        {
            configItemPanel.Children.Clear();
            foreach (var configItem in configSection.GetType().GetProperties())
            {
                var displayNameAttribute = (NameAttribute)Attribute.GetCustomAttribute(configItem, typeof(NameAttribute));
                if(displayNameAttribute != null)
                {
                    Binding b = new Binding(configItem.Name);
                    b.Source = configSection;
                    b.Mode = BindingMode.TwoWay;
                    b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                    var requiredAttribute = (RequiredAttribute)Attribute.GetCustomAttribute(configItem, typeof(RequiredAttribute));
                    var editorTypeAttribute = (ConfigEditorAttribute)Attribute.GetCustomAttribute(configItem, typeof(ConfigEditorAttribute));

                    if (ConfigSection.Guid == new Guid("FEACBCE2-8290-4D90-BB05-373B9D7DBBFC") && configItem.Name == "ExcludedIdentifiers")
                    {
                        var visibilityControl = new ConfigVisibilityControl(_localController.GetLocalStrings<SDGuiStrings>(), _coreConfigSection, _buildController);
                        visibilityControl.ConfigItemDisplayName = _localController.GetLocalString(displayNameAttribute.LocalType, displayNameAttribute.DisplayName);

                        configItemPanel.Children.Add(visibilityControl);
                    }
                    else if (editorTypeAttribute != null && editorTypeAttribute.Editor == EditorType.CheckBoxList 
                        && ConfigSection.Guid == new Guid("FEACBCE2-8290-4D90-BB05-373B9D7DBBFC")
                        && configItem.Name == "ActivatedExporters")
                    {
                        var exporterList = new CheckBoxList();
                        foreach (var exporter in _allExporters)
                        {
                            exporterList.Add(exporter.ExporterName);
                        }

                        var checkBoxListControl = new ConfigCheckBoxListControl(_localController.GetLocalStrings<SDGuiStrings>());
                        checkBoxListControl.ConfigItemDisplayName = _localController.GetLocalString(displayNameAttribute.LocalType, displayNameAttribute.DisplayName);
                        checkBoxListControl.SourceList = exporterList;
                        checkBoxListControl.SetBinding(ConfigCheckBoxListControl.SelectedItemsProperty, b);

                        configItemPanel.Children.Add(checkBoxListControl);
                    }
                    else if(editorTypeAttribute == null && configItem.PropertyType == typeof(string))
                    {          
                        var textItemControl = new ConfigTextControl();
                        textItemControl.ConfigItemDisplayName = _localController.GetLocalString(displayNameAttribute.LocalType, displayNameAttribute.DisplayName);
                        textItemControl.SetBinding(ConfigTextControl.ConfigItemValueProperty, b);
                        textItemControl.WaterMarkText = requiredAttribute != null ? _localController.GetLocalStrings<SDGuiStrings>().Mandatory
                                                                                    : _localController.GetLocalStrings<SDGuiStrings>().Optional;
                        textItemControl.WaterMarkColor = requiredAttribute != null ? (SolidColorBrush)TryFindResource("Color_FadedRed") : (SolidColorBrush)TryFindResource("Color_FadedGray");

                        configItemPanel.Children.Add(textItemControl);
                    }
                    else if (editorTypeAttribute == null && configItem.PropertyType == typeof(bool))
                    {
                        var boolItemControl = new ConfigBoolControl();
                        boolItemControl.ConfigItemDisplayName = _localController.GetLocalString(displayNameAttribute.LocalType, displayNameAttribute.DisplayName);
                        boolItemControl.SetBinding(ConfigBoolControl.ConfigItemValueProperty, b);

                        configItemPanel.Children.Add(boolItemControl);
                    }
                    else if (editorTypeAttribute != null && editorTypeAttribute.Editor == EditorType.ComboBox && editorTypeAttribute.SourceListType != null)
                    {
                        var dropDownControl = new ConfigComboBoxControl();
                        dropDownControl.ConfigItemDisplayName = _localController.GetLocalString(displayNameAttribute.LocalType, displayNameAttribute.DisplayName);
                        dropDownControl.SourceList = (ComboBoxList)Activator.CreateInstance(editorTypeAttribute.SourceListType);
                        dropDownControl.SetBinding(ConfigComboBoxControl.SelectedValueProperty, b);
                        dropDownControl.WaterMarkText = requiredAttribute != null ? _localController.GetLocalStrings<SDGuiStrings>().Mandatory
                                                                                    : _localController.GetLocalStrings<SDGuiStrings>().Optional;
                        dropDownControl.WaterMarkColor = requiredAttribute != null ? (SolidColorBrush)TryFindResource("Color_FadedRed") : (SolidColorBrush)TryFindResource("Color_FadedGray");

                        configItemPanel.Children.Add(dropDownControl);
                    }
                    else if (editorTypeAttribute != null && editorTypeAttribute.Editor == EditorType.Colorpicker)
                    {
                        var colorControl = new ConfigColorControl();
                        colorControl.ConfigItemDisplayName = _localController.GetLocalString(displayNameAttribute.LocalType, displayNameAttribute.DisplayName);
                        colorControl.SetBinding(ConfigColorControl.ConfigItemValueProperty, b);

                        configItemPanel.Children.Add(colorControl);
                    }
                    else if (editorTypeAttribute != null && (editorTypeAttribute.Editor == EditorType.Filepicker || editorTypeAttribute.Editor == EditorType.Folderpicker))
                    {
                        var fileSystemControl = new ConfigFileSystemControl();
                        fileSystemControl.ConfigItemDisplayName = _localController.GetLocalString(displayNameAttribute.LocalType, displayNameAttribute.DisplayName);
                        fileSystemControl.SetBinding(ConfigFileSystemControl.ConfigItemValueProperty, b);
                        fileSystemControl.WaterMarkText = requiredAttribute != null ? _localController.GetLocalStrings<SDGuiStrings>().Mandatory
                                                                                    : _localController.GetLocalStrings<SDGuiStrings>().Optional;
                        fileSystemControl.WaterMarkColor = requiredAttribute != null ? (SolidColorBrush)TryFindResource("Color_FadedRed") : (SolidColorBrush)TryFindResource("Color_FadedGray");
                        fileSystemControl.IsFileSelector = editorTypeAttribute.Editor == EditorType.Filepicker;

                        configItemPanel.Children.Add(fileSystemControl);
                    }
                }
            }
        }

        private IConfigSection _configSection;
        public IConfigSection ConfigSection
        {
            get { return _configSection; }
            set 
            { 
                _configSection = value;
                ConfigSectionChanged(value);
            }
        }

        public string SectionHeader
        {
            get { return (string)GetValue(SectionHeaderProperty); }
            set { SetValue(SectionHeaderProperty, value); }
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
    }
}
