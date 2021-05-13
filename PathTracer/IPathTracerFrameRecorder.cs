namespace PathTracer
{
    public interface IPathTracerFrameRecorder
    {
        #region Methods

        void AddFrame(int width, int height);

        string Complete();

        void SetPixel(int x, int y, PathTracerColor color);

        #endregion
    }
}