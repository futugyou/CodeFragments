
using System.Text.Json.Serialization;

namespace AspnetcoreEx.KernelService.CompanyReports;

public class DoclingRoot
{
    [JsonPropertyName("schema_name")]
    public string SchemaName { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("origin")]
    public DoclingOrigin Origin { get; set; }

    [JsonPropertyName("furniture")]
    public DoclingFurniture Furniture { get; set; }

    [JsonPropertyName("body")]
    public DoclingBody Body { get; set; }

    [JsonPropertyName("groups")]
    public List<DoclingGroup> Groups { get; set; }

    [JsonPropertyName("texts")]
    public List<DoclingText> Texts { get; set; }

    [JsonPropertyName("pictures")]
    public List<DoclingPicture> Pictures { get; set; }

    [JsonPropertyName("tables")]
    public List<Table> Tables { get; set; }

    [JsonPropertyName("key_value_items")]
    public List<object> KeyValueItems { get; set; }

    [JsonPropertyName("form_items")]
    public List<object> FormItems { get; set; }

    [JsonPropertyName("pages")]
    public Dictionary<int, DoclingPage> Pages { get; set; }
}

public class DoclingPage
{
    [JsonPropertyName("size")]
    public DoclingSize Size { get; set; }

    [JsonPropertyName("page_no")]
    public int PageNo { get; set; }
}

public class DoclingBbox
{
    [JsonPropertyName("l")]
    public double L { get; set; }

    [JsonPropertyName("t")]
    public double T { get; set; }

    [JsonPropertyName("r")]
    public double R { get; set; }

    [JsonPropertyName("b")]
    public double B { get; set; }

    [JsonPropertyName("coord_origin")]
    public string CoordOrigin { get; set; }
}

public class DoclingBody
{
    [JsonPropertyName("self_ref")]
    public string SelfRef { get; set; }

    [JsonPropertyName("children")]
    public List<DoclingChild> Children { get; set; }

    [JsonPropertyName("content_layer")]
    public string ContentLayer { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }
}

public class DoclingCaption
{
    [JsonPropertyName("$ref")]
    public string Ref { get; set; }
}

public class DoclingChild
{
    [JsonPropertyName("$ref")]
    public string Ref { get; set; }
}

public class DoclingData
{
    [JsonPropertyName("table_cells")]
    public List<DoclingTableCell> TableCells { get; set; }

    [JsonPropertyName("num_rows")]
    public int NumRows { get; set; }

    [JsonPropertyName("num_cols")]
    public int NumCols { get; set; }

    [JsonPropertyName("grid")]
    public List<List<DoclingTableCell>> Grids { get; set; }
}

public class DoclingFurniture
{
    [JsonPropertyName("self_ref")]
    public string SelfRef { get; set; }

    [JsonPropertyName("children")]
    public List<object> Children { get; set; }

    [JsonPropertyName("content_layer")]
    public string ContentLayer { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }
}

public class DoclingGroup
{
    [JsonPropertyName("self_ref")]
    public string SelfRef { get; set; }

    [JsonPropertyName("parent")]
    public DoclingParent Parent { get; set; }

    [JsonPropertyName("children")]
    public List<DoclingChild> Children { get; set; }

    [JsonPropertyName("content_layer")]
    public string ContentLayer { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }
}

public class DoclingOrigin
{
    [JsonPropertyName("mimetype")]
    public string Mimetype { get; set; }

    [JsonPropertyName("binary_hash")]
    public long BinaryHash { get; set; }

    [JsonPropertyName("filename")]
    public string Filename { get; set; }
}

public class DoclingParent
{
    [JsonPropertyName("$ref")]
    public string Ref { get; set; }
}

public class DoclingPicture
{
    [JsonPropertyName("self_ref")]
    public string SelfRef { get; set; }

    [JsonPropertyName("parent")]
    public DoclingParent Parent { get; set; }

    [JsonPropertyName("children")]
    public List<object> Children { get; set; }

    [JsonPropertyName("content_layer")]
    public string ContentLayer { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("prov")]
    public List<DoclingProv> Prov { get; set; }

    [JsonPropertyName("captions")]
    public List<object> Captions { get; set; }

    [JsonPropertyName("references")]
    public List<object> References { get; set; }

    [JsonPropertyName("footnotes")]
    public List<object> Footnotes { get; set; }

    [JsonPropertyName("annotations")]
    public List<object> Annotations { get; set; }
}

public class DoclingProv
{
    [JsonPropertyName("page_no")]
    public int PageNo { get; set; }

    [JsonPropertyName("bbox")]
    public DoclingBbox Bbox { get; set; }

    [JsonPropertyName("charspan")]
    public List<int> Charspan { get; set; }
}

public class DoclingSize
{
    [JsonPropertyName("width")]
    public double Width { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; }
}

public class DoclingTable
{
    [JsonPropertyName("self_ref")]
    public string SelfRef { get; set; }

    [JsonPropertyName("parent")]
    public DoclingParent Parent { get; set; }

    [JsonPropertyName("children")]
    public List<DoclingChild> Children { get; set; }

    [JsonPropertyName("content_layer")]
    public string ContentLayer { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("prov")]
    public List<DoclingProv> Prov { get; set; }

    [JsonPropertyName("captions")]
    public List<DoclingCaption> Captions { get; set; }

    [JsonPropertyName("references")]
    public List<object> References { get; set; }

    [JsonPropertyName("footnotes")]
    public List<object> Footnotes { get; set; }

    [JsonPropertyName("data")]
    public DoclingData Data { get; set; }

    [JsonPropertyName("annotations")]
    public List<object> Annotations { get; set; }
}

public class DoclingTableCell
{
    [JsonPropertyName("bbox")]
    public DoclingBbox Bbox { get; set; }

    [JsonPropertyName("row_span")]
    public int RowSpan { get; set; }

    [JsonPropertyName("col_span")]
    public int ColSpan { get; set; }

    [JsonPropertyName("start_row_offset_idx")]
    public int StartRowOffsetIdx { get; set; }

    [JsonPropertyName("end_row_offset_idx")]
    public int EndRowOffsetIdx { get; set; }

    [JsonPropertyName("start_col_offset_idx")]
    public int StartColOffsetIdx { get; set; }

    [JsonPropertyName("end_col_offset_idx")]
    public int EndColOffsetIdx { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("column_header")]
    public bool ColumnHeader { get; set; }

    [JsonPropertyName("row_header")]
    public bool RowHeader { get; set; }

    [JsonPropertyName("row_section")]
    public bool RowSection { get; set; }
}

public class DoclingText
{
    [JsonPropertyName("self_ref")]
    public string SelfRef { get; set; }

    [JsonPropertyName("parent")]
    public DoclingParent Parent { get; set; }

    [JsonPropertyName("children")]
    public List<object> Children { get; set; }

    [JsonPropertyName("content_layer")]
    public string ContentLayer { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("prov")]
    public List<DoclingProv> Prov { get; set; }

    [JsonPropertyName("orig")]
    public string Orig { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("enumerated")]
    public bool? Enumerated { get; set; }

    [JsonPropertyName("marker")]
    public string Marker { get; set; }
}

