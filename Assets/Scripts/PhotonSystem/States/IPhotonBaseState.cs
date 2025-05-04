using System.Threading.Tasks;
using Fusion;

namespace PhotonSystem.States
{
    public interface IPhotonBaseState
    {
        public void EnterState(PhotonManager photonManager);
        public void ExitState(PhotonManager photonManager);
    }
}