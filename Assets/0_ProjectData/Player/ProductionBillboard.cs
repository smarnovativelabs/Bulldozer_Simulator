using UnityEngine;
namespace Bulldozer.Player
{
    public sealed class ProductionBillboard : MonoBehaviour
    {
        Camera view;
        void LateUpdate(){if(!view)view=Camera.main;if(view)transform.rotation=view.transform.rotation;}
    }
}
