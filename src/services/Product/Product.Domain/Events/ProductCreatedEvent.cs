using Core.Events.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Domain.Events
{
    public class ProductCreatedEvent : IIntegrationEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOn { get; }
        public string Name { get; }
        public decimal Price { get; }

        public ProductCreatedEvent(string name, decimal price)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            Name = name;
            Price = price;
        }
    }
}
