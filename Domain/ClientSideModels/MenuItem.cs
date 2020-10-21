using System;

namespace Domain.ClientSideModels
{
    public class MenuItem
    {
        public bool IsActive { get; set; }
        public string IconColor { get; set; }
        public string Label { get; set; }
        public Guid ReferenceId { get; set; }
        public string DragCss { get; set; }
    }
}
