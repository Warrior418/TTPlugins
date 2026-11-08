using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using TigerTrade.Core.UI.Common;
using Xunit;

namespace IndicatorsPlusPlus.Delta.Tests;

public class DeltaSettingsCompatibilityTests
{
    [Fact]
    public void Tiers_and_palettes_match_the_7_4_collection_editor()
    {
        AssertEditableList(typeof(DeltaTier));
        AssertEditableList(typeof(Palette));
    }

    [Fact]
    public void Saved_member_names_are_unchanged()
    {
        AssertDataMembers(
            typeof(DeltaTier),
            "Show",
            "Size",
            "DrawingSize",
            "PaletteName",
            "ShowText",
            "Zoom"
        );
        AssertDataMembers(
            typeof(Palette),
            "Name",
            "AskBackgroundColor",
            "AskBorderColor",
            "BidBackgroundColor",
            "BidBorderColor",
            "BorderThickness",
            "BorderStyle",
            "AskTextAlignment",
            "BidTextAlignment",
            "FontSize",
            "TextLineWidth",
            "TextLineThickness",
            "TextLineStyle"
        );
        AssertDataMembers(typeof(TierZoomOptions), "ReduceRatio", "MinRatio");
    }

    [Fact]
    public void Tier_zoom_stays_serialized_and_its_fields_are_editable_scalars()
    {
        PropertyInfo zoom = typeof(DeltaTier).GetProperty(nameof(DeltaTier.Zoom));
        Assert.NotNull(zoom);
        Assert.Equal("Zoom", zoom.GetCustomAttribute<DataMemberAttribute>()?.Name);
        Assert.False(zoom.GetCustomAttribute<BrowsableAttribute>()?.Browsable ?? true);

        DeltaTier tier = new DeltaTier();
        tier.ReduceRatio = 4.5;
        tier.MinRatio = 0.25;

        Assert.Equal(4.5, tier.Zoom.ReduceRatio);
        Assert.Equal(0.25, tier.Zoom.MinRatio);
        Assert.Equal(typeof(double), typeof(DeltaTier).GetProperty(nameof(DeltaTier.ReduceRatio)).PropertyType);
        Assert.Equal(typeof(double), typeof(DeltaTier).GetProperty(nameof(DeltaTier.MinRatio)).PropertyType);
    }

    [Fact]
    public void New_items_can_be_created_the_way_the_manager_creates_them()
    {
        Assert.NotNull(typeof(Palette).GetConstructor(Type.EmptyTypes));

        DeltaTier tier = (DeltaTier)Activator.CreateInstance(typeof(DeltaTier), nonPublic: true);

        Assert.True(tier.Show);
        Assert.Equal("Default", tier.PaletteName);
        Assert.NotNull(tier.Zoom);
    }

    [Fact]
    public void Source_keeps_list_contracts_for_templates()
    {
        string delta = File.ReadAllText(SourcePath("Delta++.cs"));
        string advanced = File.ReadAllText(SourcePath("AdvancedOptions/AdvancedOptions.cs"));

        Assert.Contains("[DataMember(Name = \"DeltaTiers\")]", delta);
        Assert.Contains("public List<DeltaTier> DeltaTiers", delta);
        Assert.Contains("[DataMember(Name = \"Palettes\")]", delta);
        Assert.Contains("public List<Palette> Palettes", delta);
        Assert.Contains("[DataMember(Name = \"AdvancedOptions\")]", delta);
        Assert.Contains("[Browsable(false)]", delta);
        Assert.Contains("[DataMember(Name = \"StrictTiers\")]", advanced);
        Assert.Contains("[DataMember(Name = \"HideMissingDataBanner\")]", advanced);
        Assert.Contains("[DataMember(Name = \"Zoom\")]", advanced);
    }

    private static string SourcePath(string relative)
    {
        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Delta++", relative)
        );
    }

    private static void AssertEditableList(Type itemType)
    {
        Assert.True(itemType.IsClass);
        Assert.False(itemType.IsAbstract);
        Assert.True(typeof(IDynamicProperty).IsAssignableFrom(itemType));
        Assert.NotNull(itemType.GetConstructor(Type.EmptyTypes));

        Type listType = typeof(List<>).MakeGenericType(itemType);
        Assert.True(typeof(IList).IsAssignableFrom(listType));
        Assert.True(listType.IsGenericType);
        Assert.Single(listType.GetGenericArguments());
        Assert.Equal(itemType, EditableItemType(listType));
    }

    // Same predicate as TigerTrade 7.4.9805 indicator manager (vgsPgkrMl7Y):
    // IList<T> of one concrete class that implements IDynamicProperty.
    private static Type EditableItemType(Type type)
    {
        if (type == null || !typeof(IList).IsAssignableFrom(type) || !type.IsGenericType)
        {
            return null;
        }

        Type[] args = type.GetGenericArguments();
        if (args.Length != 1)
        {
            return null;
        }

        Type item = args[0];
        if (!item.IsClass || item.IsAbstract || !typeof(IDynamicProperty).IsAssignableFrom(item))
        {
            return null;
        }

        return item;
    }

    private static void AssertDataMembers(Type type, params string[] names)
    {
        string[] actual = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.GetCustomAttribute<DataMemberAttribute>()?.Name)
            .Where(name => name != null)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(names.OrderBy(name => name, StringComparer.Ordinal).ToArray(), actual);
    }
}
