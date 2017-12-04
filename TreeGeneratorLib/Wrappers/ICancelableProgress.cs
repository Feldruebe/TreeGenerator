namespace TreeGeneratorLib.Wrappers
{
    using System;

    public interface ICancelableProgress : IProgress<string>
    {
        bool CancelRequested();
    }
}