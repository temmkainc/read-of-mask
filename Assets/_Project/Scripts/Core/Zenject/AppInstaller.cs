using UnityEngine;
using Zenject;

public class AppInstaller : MonoInstaller
{
    [SerializeField] private MaskModule.ConfigData _maskModuleConfigData;
    [SerializeField] private CoreModule.ConfigData _coreModuleConfigData;

    public override void InstallBindings()
    {
        CoreModule.Install(Container, _coreModuleConfigData);
        MaskModule.Install(Container, _maskModuleConfigData);
    }
}