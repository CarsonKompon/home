namespace Home.Util;

public static class ClothingHelper
{
    private static void LoadCloudModels(ref ClothingContainer container)
    {
        foreach (var cloth in container.Clothing)
        {
            if(cloth is not HomeClothing homeCloth) continue;
            if(!string.IsNullOrEmpty(homeCloth.CloudModel))
            {
                homeCloth.Model = Cloud.Model(homeCloth.CloudModel).Name;
                homeCloth.CloudModel = "";
            }
        }
    }

    public static void DressEntity(AnimatedEntity citizen, ClothingContainer container, bool hideInFirstPerson = true, bool castShadowsInFirstPerson = true)
    {
        LoadCloudModels(ref container);
        container.DressEntity(citizen, hideInFirstPerson, castShadowsInFirstPerson);
    }

    public static List<SceneModel> DressSceneObject(SceneModel citizen, ClothingContainer container)
    {
        LoadCloudModels(ref container);
        return container.DressSceneObject(citizen);
    }
}
