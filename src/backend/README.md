# Joseki Backend

## Getting started

The project uses `dotnet core 3.1`.

### From cli

To build the sources: `dotnet build ./src/backend/joseki.be/`

To run unit tests: `dotnet test ./src/backend/joseki.be/`

To run locally: `dotnet run --project ./src/backend/joseki.be/webapp/webapp.csproj`

### From docker

To run the application:

```bash
docker build --target webapp joseki-be:latest .
docker run -p 5000:8080 --rm -it joseki-be:latest
```

### Swagger

When service is running, it serves swagger interface at `http://localhost:5000/swagger`.

The used models documentation could be found at `Schemas` section:

![Swagger schema documentation](/docs/backend/swagger-docs.png)
