using System;

using core.Exceptions;

using Newtonsoft.Json;

namespace core.core
{
    public class ContainerImage : IEquatable<ContainerImage>
    {
        public ContainerImage(string fullName)
        {
            this.FullName = fullName;
        }

        public static ContainerImage FromFullName(string fullName)
        {
            var image = new ContainerImage(fullName);

            var parts = fullName.Split(':');
            switch (parts.Length)
            {
                case 1:
                    image.Repository = fullName;
                    image.Tag = "latest";
                    break;
                case 2:
                    image.Repository = parts[0];
                    image.Tag = parts[1];
                    break;
                default:
                    throw new WrongImageTagFormatException(fullName);
            }

            var repositoryParts = image.Repository.Split('/');
            if (repositoryParts.Length > 0)
            {
                image.ContainerRegistry = repositoryParts[0];
            }

            return image;
        }

        [JsonProperty(PropertyName = "containerRegistry")]
        public string ContainerRegistry { get; private set; }

        [JsonProperty(PropertyName = "repository")]
        public string Repository { get; private set; }

        [JsonProperty(PropertyName = "tag")]
        public string Tag { get; private set; }

        [JsonProperty(PropertyName = "fullName")]
        public string FullName { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.FullName;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is ContainerImage containerObj))
            {
                return false;
            }

            return this.Equals(containerObj);
        }

        /// <inheritdoc />
        public bool Equals(ContainerImage other)
        {
            return other != null && this.FullName == other.FullName;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.FullName.GetHashCode();
        }

        public static bool operator ==(ContainerImage left, ContainerImage right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ContainerImage left, ContainerImage right)
        {
            return !Equals(left, right);
        }
    }
}