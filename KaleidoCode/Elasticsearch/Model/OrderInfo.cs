using OpenSearch.Client;

namespace KaleidoCode.Elasticsearch;

[OpenSearchType(RelationName = "order")]
public class OrderInfo
{
    [Keyword(Name = "Id")]
    public string Id { get; set; }
    [Date(Name = "CreateTime")]
    public DateTime CreateTime { get; set; }
    [Keyword]
    public string Name { get; set; }
    //[PropertyName("goods_name")]
    //[DataMember(Name = "goods_name")]
    [Text(Name = "goods_name")]
    public string GoodsName { get; set; }
    public string Status { get; set; }
    public double Price { get; set; }
}