using System;

namespace Sonic.Models.Base
{
    /// <summary>
    /// Declares that this entity can inherit ownership from related entities
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CascadeOwnershipFromAttribute : Attribute
    {
        /// <summary>
        /// The property name that references the owning entity
        /// </summary>
        public string PropertyName { get; }
        
        /// <summary>
        /// The type of the owning entity
        /// </summary>
        public Type OwningEntityType { get; }
        
        /// <summary>
        /// Priority level for ownership resolution (lower numbers = higher priority)
        /// Used when multiple ownership paths exist
        /// </summary>
        public int Priority { get; set; } = 100;
        
        /// <summary>
        /// Whether this ownership relationship is required for the entity to exist
        /// </summary>
        public bool IsRequired { get; set; } = false;

        public CascadeOwnershipFromAttribute(string propertyName, Type owningEntityType)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            OwningEntityType = owningEntityType ?? throw new ArgumentNullException(nameof(owningEntityType));
        }
    }

    /// <summary>
    /// Declares that this entity can grant ownership to related entities
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CascadeOwnershipToAttribute : Attribute
    {
        /// <summary>
        /// The property name that references entities that inherit ownership
        /// </summary>
        public string PropertyName { get; }
        
        /// <summary>
        /// The type of entities that inherit ownership
        /// </summary>
        public Type InheritingEntityType { get; }
        
        /// <summary>
        /// The membership types that are cascaded (if null, all memberships cascade)
        /// </summary>
        public MembershipType[]? CascadedMemberships { get; set; }

        public CascadeOwnershipToAttribute(string propertyName, Type inheritingEntityType)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            InheritingEntityType = inheritingEntityType ?? throw new ArgumentNullException(nameof(inheritingEntityType));
        }
    }

    /// <summary>
    /// Declares that this entity supports direct ownership (can have ResourceMembership records)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DirectOwnershipAttribute : Attribute
    {
        /// <summary>
        /// The resource type used for this entity in ResourceMembership records
        /// </summary>
        public ResourceType ResourceType { get; }

        public DirectOwnershipAttribute(ResourceType resourceType)
        {
            ResourceType = resourceType;
        }
    }
}
