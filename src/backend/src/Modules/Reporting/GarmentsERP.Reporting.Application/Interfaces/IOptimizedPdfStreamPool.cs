namespace GarmentsERP.Reporting.Application.Interfaces;

public interface IOptimizedPdfStreamPool
{
    /// <summary>
    /// Obtains a pooled, recyclable MemoryStream from Microsoft.IO.RecyclableMemoryStreamManager.
    /// Eliminates LOH (Large Object Heap) fragmentation and disk I/O during high-frequency PDF generation.
    /// </summary>
    MemoryStream GetRecyclableStream(string tag);

    /// <summary>
    /// Obtains a pooled stream initialized with an expected capacity.
    /// </summary>
    MemoryStream GetRecyclableStream(string tag, int requiredSize);
}
