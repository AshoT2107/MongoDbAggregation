
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

Console.WriteLine("Hello, World!");

var client = new MongoClient("mongodb://localhost:27017");

var database = client.GetDatabase($"spot-{DateTime.Now:ddMMyyyy}");

var customersCollection = database.GetCollection<BsonDocument>("Customer");
var ordersCollection = database.GetCollection<Order>("Order");

/*// Insert sample data into the orders collection
ordersCollection.InsertMany(new List<Order>()
{
    new Order
    {
        Product = "product1",
    },
    new Order
    {
        Product = "product2",
    },
    new Order
    {
        Product = "product3",
    },
    new Order
    {
        Product = "product4",
    },
    new Order
    {
        Product = "product5",
    }
});

// Insert sample data into the customers collection
customersCollection.InsertMany(new List<Customer>()
{
    new Customer
    {
        Name = "name1",
        Email = "email1"
    },
    new Customer
    {
        Name = "name2",
        Email = "email2"
    },
});*/

#region By C# Methods 

List<BsonDocument> pResults = customersCollection.Aggregate()
    .Match(new BsonDocument()
    {
        {
            "Name", "name1"
        }
    })
    .Project(new BsonDocument
        {
            {"_id", 1 },
            {"Name", 1 },
            {
                "OrderIds", new BsonDocument
                {
                    {
                        "$map", new BsonDocument
                        {
                            {"input", "$OrderIds" },
                            { "as", "orderId"},
                            {
                                "in", new BsonDocument
                                {
                                    {
                                        "$convert", new BsonDocument
                                        {
                                            {"input", "$$orderId"},
                                            {"to", "objectId" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        })
    .Lookup("Order", "OrderIds", "_id", "Orders")
    .Project(new BsonDocument
        {
            {"_id", 1 },
            {"Name", 1 },
            {"Orders", 1 }
        })
    .ToList();

foreach (var pResult in pResults)
{
    var item = new CustomerModel()
    {
        Id = pResult["_id"].AsObjectId,
        Name = pResult["Name"].AsString,
        Orders = pResult["Orders"].AsBsonArray.Select(orderDoc => new OrderModel
        {
            Id = orderDoc["_id"].AsObjectId,
            Product = orderDoc["Product"].AsString
        }).ToList()
    };
    
    Console.WriteLine(pResult);
}
#endregion

#region By MongoDb Pipeline
/*BsonDocument pipelineStage1 = new BsonDocument
{
    {
        "$match", new BsonDocument
        {
            { "Name", "name1" }
        }
    }
};

BsonDocument pipelineStage2 = new BsonDocument
{
    {
        "$project", new BsonDocument
        {
            {"_id", 1 },
            {"Name", 1 },
            {
                "OrderIds", new BsonDocument
                {
                    {
                        "$map", new BsonDocument
                        {
                            {"input", "$OrderIds" },
                            { "as", "orderId"},
                            {
                                "in", new BsonDocument
                                {
                                    {
                                        "$convert", new BsonDocument
                                        {
                                            {"input", "$$orderId"},
                                            {"to", "objectId" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
};

BsonDocument pipelineStage3 = new BsonDocument
{
    {
        "$lookup", new BsonDocument
        {
            {"from", "Order" },
            {"localField", "OrderIds" },
            {"foreignField", "_id" },
            {"as", "Orders" }
        }
    }
};

BsonDocument pipelineStage4 = new BsonDocument
{
    {
        "$project", new BsonDocument
        {
            {"_id", 1 },
            {"Name", 1 },
            {"Orders", 1 }
        }
    }
};

BsonDocument[] pipeline = new BsonDocument[] { pipelineStage1, pipelineStage2, pipelineStage3, pipelineStage4 };

List<BsonDocument> pResults = customersCollection.Aggregate<BsonDocument>(pipeline).ToList();

foreach (var pResult in pResults)
{
    Console.WriteLine(pResult);
}*/
#endregion

Console.ReadKey();

class Customer
{

    public string Name { get; set; }

    public string Email { get; set; }

    public List<string> OrderIds { get; set; }
}

class Order
{

    public string Product { get; set; }
}


public class CustomerModel
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string Name { get; set; }

    public List<OrderModel> Orders { get; set; }
}

public class OrderModel
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string Product { get; set; }
}