using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// moj class k extenda base NetworkContainerBehaviour class on forge networkinga. extendan je zato ker nesmemo spreminjat kode od networkinga ker se avtomatsko generira in bi nam posral / zbrisal kodo
/// </summary>
public class NetworkContainer : NetworkContainerBehavior
{
    #region RPC
    public override void ContainerToContainer(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void BackpackToContainer(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void BarToContainer(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void ContainerToBackpack(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void ContainerToBar(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void ContainerToLoadout(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void ContainerToPersonal(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void LoadoutToContainer(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void openRequest(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void openResponse(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void PersonalToContainer(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void pickupRequest(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void dropItem(RpcArgs args)
    {
        throw new NotImplementedException();
    }
    #endregion
    #region LOCAL CALLS

    internal virtual void localRequestPersonalToContainer(int indexFrom, int indexTo) {
        throw new NotImplementedException();
    }

    internal virtual void localRequestBackpackToContainer(int indexFrom, int indexTo)
    {
        throw new NotImplementedException();
    }

    internal virtual void localRequestContainerToBackpack(int indexFrom, int indexTo)
    {
        throw new NotImplementedException();
    }

    internal virtual void localRequestContainerToPersonal(int indexFrom, int indexTo)
    {
        throw new NotImplementedException();
    }

    internal virtual void localRequestBarToContainer(int indexFrom, int indexTo)
    {
        throw new NotImplementedException();
    }

    internal virtual void localRequestContainerToBar(int indexFrom, int indexTo)
    {
        throw new NotImplementedException();
    }

    internal virtual void localRequestLoadoutToContainer(int indexFrom, int indexTo)
    {
        throw new NotImplementedException();
    }

    internal virtual void localRequestContainerToLoadout(int indexFrom, int indexTo)
    {
        throw new NotImplementedException();
    }

    internal virtual void localRequestContainerToContainer(int indexFrom, int indexTo)
    {
        throw new NotImplementedException();
    }

    internal virtual void localRequestDropItemContainer(int v)
    {
        throw new NotImplementedException();
    }
    #endregion
}
