using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Биндим интерфейс ITaskManager к конкретной реализации TaskManager на сцене
        Container.Bind<ITaskManager>()
            .To<TaskManager>()
            .FromComponentInHierarchy()
            .AsSingle();

        // Биндим интерфейс IPlayerInventory к конкретной реализации PlayerInventory на сцене
        Container.Bind<IPlayerInventory>()
            .To<PlayerInventory>()
            .FromComponentInHierarchy()
            .AsSingle();
    }
}