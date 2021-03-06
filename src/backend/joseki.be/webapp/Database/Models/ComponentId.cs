﻿using System;

namespace webapp.Database.Models
{
#pragma warning disable CS1591
    /// <summary>
    /// Component can be either Kubernetes or AzureSubscription.
    /// </summary>
    public enum ComponentType
    {
        Kubernetes,
        AzureSubscription,
    }

    /// <summary>
    /// Base class for parsing ComponentId.
    /// </summary>
    public class ComponentId
    {
        public const string StrSubscription = "subscriptions";
        public const string StrResourceGroup = "resource_group";
        public const string StrK8s = "k8s";
        public const string StrNamespace = "ns";

        public ComponentType Type { get; set; }

        public string[] Parts { get; set; }

        public ComponentId(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new Exception("Identifier empty");
            }

            if (!identifier.StartsWith($"/{StrSubscription}/") && !identifier.StartsWith($"/{StrK8s}/"))
            {
                throw new Exception("Invalid identifier");
            }

            this.Parts = identifier.Split('/');
        }

        public static IComponentId ComponentIdFactory(string identifier)
        {
            var componentId = new ComponentId(identifier);
            switch (componentId.Parts[1])
            {
                case StrSubscription:
                    return new AzureComponentId(identifier);
                case StrK8s:
                    return new KubernetesComponentId(identifier);
                default:
                    throw new Exception("Invalid identifier");
            }
        }
    }

    /// <summary>
    /// Interface for ComponentId, returning 3 main segments of the Id.
    /// Uses an hierarchial lookup favoring the more detailed Id.
    /// </summary>
    public interface IComponentId
    {
        string RootLevel { get; }

        string GroupLevel { get; }

        string ObjectLevel { get; }
    }

    /// <summary>
    /// ComponentId for Azure Subscription.
    /// </summary>
    public class AzureComponentId : ComponentId, IComponentId
    {
        public string SubscriptionId { get; set; }

        public string ResourceGroupName { get; set; }

        public string ObjectType { get; set; }

        public string ObjectName { get; set; }

        public AzureComponentId(string identifier)
            : base(identifier)
        {
            this.SubscriptionId = this.Parts[2];
            if (this.Parts.Length > 4)
            {
                this.ResourceGroupName = this.Parts[4];
            }

            if (this.Parts.Length > 5)
            {
                this.ObjectType = this.Parts[5];
                this.ObjectName = this.Parts[6];
            }
        }

        public string RootLevel
        {
            get => $"/{StrSubscription}/{this.SubscriptionId}";
        }

        public string GroupLevel
        {
            get => $"/{StrSubscription}/{this.SubscriptionId}/{StrResourceGroup}/{this.ResourceGroupName}";
        }

        public string ObjectLevel
        {
            get => $"/{StrSubscription}/{this.SubscriptionId}/{StrResourceGroup}/{this.ResourceGroupName}/{this.ObjectType}/{this.ObjectName}";
        }
    }

    /// <summary>
    /// ComponentId for Kubernetes Cluster.
    /// </summary>
    public class KubernetesComponentId : ComponentId, IComponentId
    {
        public string ClusterId { get; set; }

        public string Namespace { get; set; }

        public string ObjectType { get; set; }

        public string ObjectName { get; set; }

        public KubernetesComponentId(string identifier)
            : base(identifier)
        {
            this.ClusterId = this.Parts[2];

            if (this.Parts.Length > 4)
            {
                this.Namespace = this.Parts[4];
            }

            if (this.Parts.Length > 5)
            {
                this.ObjectType = this.Parts[5];
                this.ObjectName = this.Parts[6];
            }
        }

        public string RootLevel
        {
            get => $"{StrSubscription}/{this.ClusterId}";
        }

        public string GroupLevel
        {
            get => $"{StrSubscription}/{this.ClusterId}/{StrNamespace}/{this.Namespace}";
        }

        public string ObjectLevel
        {
            get => $"{StrSubscription}/{this.ClusterId}/{StrNamespace}/{this.Namespace}/{this.ObjectType}/{this.ObjectName}";
        }
    }
#pragma warning restore CS1591
}
