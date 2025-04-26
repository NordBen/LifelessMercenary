using UnityEngine;
using UnityEngine.UIElements;

public class TestingUIToolkit : MonoBehaviour
{
    [SerializeField] private UIDocument _document;
    [SerializeField] private StyleSheet _styleSheet;
/*
    private void Start()
    {
        Generate();
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        Generate();
    }

    private void Generate()
    {
        var root = _document.rootVisualElement;
        root.Clear();

        root.styleSheets.Add(_styleSheet);

        var titleLabel = new Label("Hello");
        root.Add(titleLabel);

        var headerLabel = new Label("Do Not Click");
        root.Add(headerLabel);

        var buttonTest = Create<Button>("btn");
        root.Add(buttonTest);

        var buttonTest2 = Create<Button>("btn2");
        root.Add(buttonTest2);

        var buttonTest3 = Create<Button>("btn3");
        root.Add(buttonTest3);

        var testSlider = Create<Slider>();
        root.Add(testSlider);
    }*/

    VisualElement Create(params string[] classNames)
    {
        return Create<VisualElement>(classNames);
    }

    T Create<T>(params string[] classNames) where T : VisualElement, new()
    {
        var element = new T();
        foreach (var className in classNames)
        {
            element.AddToClassList(className);
        }
        return element;
    }
}