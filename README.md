# Xperience Page Template Utilities

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.PageCustomDataControlExtender.svg)](https://www.nuget.org/packages/XperienceCommunity.PageCustomDataControlExtender)

A Kentico Xperience Form Control Extender that sync the Form Control value to/from Page CustomData fields

## Dependencies

This package is compatible with Kentico Xperience 13.

## How to Use?

1. First, install the NuGet package in your Kentico Xperience administration `CMSApp` project

   ```bash
   dotnet add package XperienceCommunity.PageCustomDataControlExtender
   ```

1. Open the Administration application and navigate to the "Administration Interface" module in the menu

   ![Administration Interface](./images/01-administration-interface.jpg)

1. Create a new Form Control, selecting to inherit from the Form Control you would like to use to store data in a Page's CustomData field

   ![New Form Control](./images/02-new-inheriting-form-control.jpg)

1. After creating the new Form Control, assign the correct settings to that the Control is available for Page Type fields of the correct data type

   ![Control Settings](./images/03-assign-control-settings.jpg)

1. (Optional) Create a custom Property for this Form Control that allows it to store data in the correct Page field location for each use:

   **Note**: If this Property is not defined, the Control will default to storing the Control value in `DocumentCustomData`

   - `UseDocumentCustomData` - Data type: Boolean, Required: true

     ![UseDocumentCustomData Field](./images/04-use-document-customdata.jpg)

1. Create a new field in your Page Type, using the new Form Control:

   Make sure you select "Field without database representation" as the `Field type`, select a `Data type` that
   your Form Control is configured to work with, and also select the new custom `Form control` in the drop down.

   **Note**: The name of the field (`ShowInSitemap` in the screenshot) is the XML element that will contain the value
   in the Page CustomData field.

   ![New Page Type field](./images/05-new-page-type-field.jpg)

1. (Optional) Register the included custom `VersionManager` to make `DocumentCustomData` a versioned Page field:

   Follow the instructions for [Registering providers via the web.config](https://docs.xperience.io/custom-development/customizing-providers/registering-providers-via-the-web-config) in the Kentico Xperience documentation.

   **Note**: By default, `DocumentCustomData` is not versioned, so changes to the field will update the `CMS_Document` table,
   even for Pages under workflow.

   ```xml
   <section name="cms.extensibility" type="CMS.Base.CMSExtensibilitySection, CMS.Base" />

   <cms.extensibility>
      <managers>
         <add name="WorkflowManager"
               assembly="XperienceCommunity.PageCustomDataControlExtender"
               type="XperienceCommunity.PageCustomDataControlExtender.CustomDataVersionManager" />
         ...
      </managers>
   </cms.extensibility>
   ```

## How Does It Work?

Normally, if we want to store data in Pages, we define fields for the Page's custom Page Type. This field often a "Standard Field" which results in a column in the Page Type field database table.

However, Pages already have 2 fields that can be used to store any kind of serializable data/content - `DocumentCustomData` and `NodeCustomData`. Data in these fields is store in XML data structures and the `TreeNode` APIs to access these fields perform the XML serialization/deserialization automatically.

If we want to store data in these fields we need a way to associate a Form Control (eg Check Box, Text Box, Media Selector) with a one of these XML structures and an XML element name in which the value of the Form Control should be stored.

Kentico Xperience's Portal Engine API (still used in the Administration application) allows for Form Control Extender classes to get access to a Form Control's data, Page, and Form during events of the Control's event lifecycle.

This NuGet package exposes a Form Control Extender that can be applied to custom Form Controls that inherit from existing ones - maintaining their original functionality, but intercepting the Value produced by the Form Control and sync it to/from the Page's CustomData field.

Since the Page Type field that the extended Form Control operates on is a "Field without database representation", the Page Type database table does not need modified and Content Managers don't lose any of the standard content management functionality they are used to.

### Querying XML via SQL

Normally Page CustomData fields should to be used to store data that we want access to when we retrieve a Page from the database, in which case the `DocumentCustomData` or `NodeCustomData` will handle the XML deserialization for you.

However, if we want to filter in SQL on these XML columns, we can cast the column to the SQL XML type and filter on its value.

Using the setup in the steps above, this query would return all Pages (of any Page Type) that have "Show In Sitemap" set to `true`.

```sql
SELECT *
FROM CMS_Document
WHERE CAST(DocumentCustomData as XML).value('(//CustomData/ShowInSitemap/text())[1]', 'bit') = 1
```

## References

### Kentico Xperience

- [Field Editor: Creating New Fields](https://docs.xperience.io/custom-development/extending-the-administration-interface/developing-form-controls/reference-field-editor#ReferenceFieldeditor-Creatingnewfields)
- [Inheriting from existing form controls](https://docs.xperience.io/custom-development/extending-the-administration-interface/developing-form-controls/inheriting-from-existing-form-controls)
- [Defining form control parameters](https://docs.xperience.io/custom-development/extending-the-administration-interface/developing-form-controls/defining-form-control-parameters)
