namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Represents alternative operation status where <see cref="OperationStatus.Finished"/> status splitted to <see cref="Success"/> and <see cref="Failed"/>.
    /// </summary>
    public enum OperationStatusWithError
    {
        /// <summary>
        /// Operation is created but not started.
        /// </summary>
        NotStarted,

        /// <summary>
        /// Operation is in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// Operation if finished successfully.
        /// </summary>
        Success,

        /// <summary>
        /// Operation if finished with error.
        /// </summary>
        Failed,
    }
}
