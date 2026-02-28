using UnityEngine;
using Zenject;

public class AppInstaller : MonoInstaller
{
    [SerializeField] private MaskModule.ConfigData _maskModuleConfigData;
    [SerializeField] private CoreModule.ConfigData _coreModuleConfigData;
    [SerializeField] private PlayerModule.ConfigData _playerModuleConfigData;
    [SerializeField] private DiaryModule.ConfigData _diaryModuleConfigData;

    public override void InstallBindings()
    {
        CoreModule.Install(Container, _coreModuleConfigData);
        PlayerModule.Install(Container, _playerModuleConfigData);
        MaskModule.Install(Container, _maskModuleConfigData);
        DiaryModule.Install(Container, _diaryModuleConfigData);
    }
}