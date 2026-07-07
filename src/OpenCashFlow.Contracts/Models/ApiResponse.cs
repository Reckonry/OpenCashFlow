using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenCashFlow.Contracts.Models
{
    public class ApiResponse<T>(bool success, string? message, [Optional] T? data, [Optional] List<string>? errors)
    {
        public bool Success { get; set; } = success;
        public string? Message { get; set; } = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
        public T? Data { get; set; } = data;
        public List<string>? Errors { get; set; } = errors ?? [];
    }
}
