using GarmentsERP.Reporting.Application.Interfaces;
using Microsoft.IO;

namespace GarmentsERP.Reporting.Infrastructure.Services;

/// <summary>
/// Implements SCH-02: In-memory QuestPDF rendering pipeline optimization with RecyclableMemoryStreamManager buffers.
/// Eliminates Large Object Heap (LOH) fragmentation and completely avoids disk I/O when generating large scheduled reports.
/// </summary>
public class OptimizedPdfStreamPool : IOptimizedPdfStreamPool
{
    private readonly RecyclableMemoryStreamManager _streamManager;

    public OptimizedPdfStreamPool()
    {
        // Configure pool parameters optimized for vector PDF rendering:
        // Block size: 128KB, Large buffer multiple: 1MB, Max buffer size: 16MB
        var options = new RecyclableMemoryStreamManager.Options
        {
            BlockSize = 128 * 1024,
            LargeBufferMultiple = 1024 * 1024,
            MaximumBufferSize = 16 * 1024 * 1024,
            GenerateCallStacks = false,
            AggressiveBufferReturn = true
        };

        _streamManager = new RecyclableMemoryStreamManager(options);
    }

    public MemoryStream GetRecyclableStream(string tag)
    {
        return _streamManager.GetStream(tag);
    }

    public MemoryStream GetRecyclableStream(string tag, int requiredSize)
    {
        return _streamManager.GetStream(tag, requiredSize);
    }
}
