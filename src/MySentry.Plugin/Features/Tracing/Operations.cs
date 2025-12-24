namespace MySentry.Plugin.Features.Tracing;

/// <summary>
/// Defines well-known operation names for spans and transactions.
/// </summary>
public static class Operations
{
    /// <summary>
    /// HTTP-related operations.
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// Incoming HTTP server request.
        /// </summary>
        public const string Server = "http.server";

        /// <summary>
        /// Outgoing HTTP client request.
        /// </summary>
        public const string Client = "http.client";
    }

    /// <summary>
    /// Database-related operations.
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Generic database query.
        /// </summary>
        public const string Query = "db.query";

        /// <summary>
        /// SQL database query.
        /// </summary>
        public const string Sql = "db.sql";

        /// <summary>
        /// Redis cache operation.
        /// </summary>
        public const string Redis = "db.redis";

        /// <summary>
        /// MongoDB operation.
        /// </summary>
        public const string Mongo = "db.mongo";

        /// <summary>
        /// Elasticsearch operation.
        /// </summary>
        public const string Elasticsearch = "db.elasticsearch";
    }

    /// <summary>
    /// Queue and messaging operations.
    /// </summary>
    public static class Queue
    {
        /// <summary>
        /// Message publish operation.
        /// </summary>
        public const string Publish = "queue.publish";

        /// <summary>
        /// Message subscribe/consume operation.
        /// </summary>
        public const string Subscribe = "queue.subscribe";

        /// <summary>
        /// Queue task processing.
        /// </summary>
        public const string Process = "queue.process";
    }

    /// <summary>
    /// Cache operations.
    /// </summary>
    public static class Cache
    {
        /// <summary>
        /// Cache get operation.
        /// </summary>
        public const string Get = "cache.get";

        /// <summary>
        /// Cache set operation.
        /// </summary>
        public const string Set = "cache.set";

        /// <summary>
        /// Cache remove operation.
        /// </summary>
        public const string Remove = "cache.remove";
    }

    /// <summary>
    /// File system operations.
    /// </summary>
    public static class FileSystem
    {
        /// <summary>
        /// File read operation.
        /// </summary>
        public const string Read = "file.read";

        /// <summary>
        /// File write operation.
        /// </summary>
        public const string Write = "file.write";

        /// <summary>
        /// File delete operation.
        /// </summary>
        public const string Delete = "file.delete";
    }

    /// <summary>
    /// Generic task operations.
    /// </summary>
    public static class Task
    {
        /// <summary>
        /// Background task.
        /// </summary>
        public const string Background = "task.background";

        /// <summary>
        /// Scheduled task.
        /// </summary>
        public const string Scheduled = "task.scheduled";

        /// <summary>
        /// Generic function execution.
        /// </summary>
        public const string Function = "task.function";
    }

    /// <summary>
    /// Serialization operations.
    /// </summary>
    public static class Serialization
    {
        /// <summary>
        /// JSON serialization.
        /// </summary>
        public const string Json = "serialize.json";

        /// <summary>
        /// XML serialization.
        /// </summary>
        public const string Xml = "serialize.xml";
    }

    /// <summary>
    /// UI-related operations.
    /// </summary>
    public static class Ui
    {
        /// <summary>
        /// UI render operation.
        /// </summary>
        public const string Render = "ui.render";

        /// <summary>
        /// View component render.
        /// </summary>
        public const string Component = "ui.component";
    }

    /// <summary>
    /// External service operations.
    /// </summary>
    public static class Service
    {
        /// <summary>
        /// gRPC call.
        /// </summary>
        public const string Grpc = "grpc";

        /// <summary>
        /// GraphQL query/mutation.
        /// </summary>
        public const string GraphQL = "graphql";

        /// <summary>
        /// Generic RPC call.
        /// </summary>
        public const string Rpc = "rpc";
    }
}
