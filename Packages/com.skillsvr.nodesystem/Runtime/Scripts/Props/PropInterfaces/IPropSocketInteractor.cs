using SkillsVRNodes.Scripts.Nodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Props.PropInterfaces
{
    public interface IPropSocketInteractor : IBaseProp
    {
        public GroupSocketFilter GetSocketFilter();
        public XRSocketInteractor GetSocketInteractor();
        public SVRColorMaterialPropertyAffordanceReceiver GetMatPropertyReciever();
        public Collider GetCollider();
        public Rigidbody GetRigidBody();
        public  void MuteInScene(DragDropNode referencer);
        public  void UnmuteInScene(DragDropNode referencer);
    }
}
