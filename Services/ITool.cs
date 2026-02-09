using SegmentationManual.Models;

namespace SegmentationManual.Services;

public interface ITool
{
    string Name { get; }
    string Description { get; }
    Task<ProcessingResult> ExecuteAsync(
        VisibilityConfiguration config,
        CancellationToken cancellationToken = default
    );
}
