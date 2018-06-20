namespace Ratcow.Csta.Engine.Core
{
    public interface IInvokeIdService
    {
        (string XmlSafeId, int InvokeId) Get();
    }
}