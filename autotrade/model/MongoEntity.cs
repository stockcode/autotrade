using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;

namespace autotrade.model
{
    /// <summary>
    ///     Abstract Entity for all the BusinessEntities.
    /// </summary>
    [DataContract]
    [Serializable]
    [BsonIgnoreExtraElements(Inherited = true)]
    public abstract class MongoEntity : IEntity<string>
    {
        /// <summary>
        ///     Gets or sets the id for this object (the primary record for an entity).
        /// </summary>
        /// <value>The id for this object (the primary record for an entity).</value>
        [Browsable(false)]
        [DataMember]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string Id { get; set; }
    }
}