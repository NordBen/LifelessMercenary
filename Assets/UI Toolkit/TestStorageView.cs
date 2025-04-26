using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class TestStorageView : MonoBehaviour
{
    public Slot[] slots;

    [SerializeField] protected UIDocument _document;
    [SerializeField] protected StyleSheet _styleSheet;

    static bool isDragging;
    static Slot originalSlot;

    protected VisualElement root;
    protected VisualElement container;


    private void OnValidate()
    {
        if (Application.isPlaying) return;
        StartCoroutine(InitializeView());
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(InitializeView());
    }

    public abstract IEnumerator InitializeView(int size = 20);

    static void OnPointerDown(Vector2 position, Slot slot)
    {
        isDragging = true;
        originalSlot = slot;
        /*
        SetGhostIconPosition(position);

        ghostIcon.style.backgroundImage = originalSlot.BaseSprite.texture;
        originalSlot.Icon.image = null;
        originalSlot.StackLabel.visible = false;

        ghostIcon.style.visibility = Visibility.Visible;*/
        // TODO show stack size on ghost icon
    }

    void OnDestroy()
    {
        foreach (var slot in slots)
        {
            slot.OnStartDrag -= OnPointerDown;
        }
    }
}

public class Slot : VisualElement
{
    public Image icon;
    public Label stackLabel;
    public int index => parent.IndexOf(this);
    //public SerializableGuid itemID { get; private set; } = SerializableGuid.Empty;
    public Sprite baseSprite;

    public event Action<Vector2, Slot> OnStartDrag = delegate { };

    public Slot()
    {
        icon = this.CreateChild<Image>("slotIcon");
        stackLabel = this.CreateChild("slotFrame").CreateChild<Label>("stackCount");
        RegisterCallback<PointerDownEvent>(OnPointerDown);
    }

    void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0 /*|| itemID.Equals(SerializableGuid.Empty)*/) return;

        OnStartDrag.Invoke(evt.position, this);
        evt.StopPropagation();
    }

    public void Set(SerializableGuid newID, Sprite newIcon, int newQuantity = 0)
    {
        //itemID = newID;
        baseSprite = newIcon;

        icon.image = baseSprite != null ? newIcon.texture : null;

        stackLabel.text = newQuantity > 1 ? newQuantity.ToString() : string.Empty;
        stackLabel.visible = newQuantity > 1;
    }

    public void Remove()
    {
        //itemID = SerializableGuid.Empty;
        icon.image = null;
    }
}

[Serializable]
public struct SerializableGuid : IEquatable<SerializableGuid>
{
    [SerializeField, HideInInspector] public uint Part1;
    [SerializeField, HideInInspector] public uint Part2;
    [SerializeField, HideInInspector] public uint Part3;
    [SerializeField, HideInInspector] public uint Part4;

    //public static SerializableGuid Empty => new(0, 0, 0, 0);

    public SerializableGuid(Guid guid)
    {
        byte[] bytes = guid.ToByteArray();
        Part1 = BitConverter.ToUInt32(bytes, 0);
        Part2 = BitConverter.ToUInt32(bytes, 4);
        Part3 = BitConverter.ToUInt32(bytes, 8);
        Part4 = BitConverter.ToUInt32(bytes, 12);
    }

    //public static SerializableGuid NewGuid() => Guid.NewGuid().ToSerializableGuid();
    /*
    public static SerializableGuid FromHexString(string hexString)
    {
        if (hexString.Length != 32)
        {
            return Empty;
        }
        /*return new SerializableGuid
        (
            Convert.ToUInt32(hexString.Substring(0, 8), 16),
            Convert.ToUInt32(hexString.Substring(8, 8), 16),
            Convert.ToUInt32(hexString.Substring(16, 8), 16),
            Convert.ToUInt32(hexString.Substring(24, 8), 16)
        );*
    }*/

    public string ToHexString()
    {
        return $"{Part1:X8}{Part2:X8}{Part3:X8}{Part4:X8}";
    }

    public Guid ToGuid()
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(Part1).CopyTo(bytes, 0);
        BitConverter.GetBytes(Part2).CopyTo(bytes, 4);
        BitConverter.GetBytes(Part3).CopyTo(bytes, 8);
        BitConverter.GetBytes(Part4).CopyTo(bytes, 12);
        return new Guid(bytes);
    }

    public static implicit operator Guid(SerializableGuid serializableGuid) => serializableGuid.ToGuid();
    public static implicit operator SerializableGuid(Guid guid) => new SerializableGuid(guid);

    public override bool Equals(object obj)
    {
        return obj is SerializableGuid guid && this.Equals(guid);
    }

    public bool Equals(SerializableGuid other)
    {
        return Part1 == other.Part1 && Part2 == other.Part2 && Part3 == other.Part3 && Part4 == other.Part4;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Part1, Part2, Part3, Part4);
    }

    public static bool operator ==(SerializableGuid left, SerializableGuid right) => left.Equals(right);
    public static bool operator !=(SerializableGuid left, SerializableGuid right) => !(left == right);
}

public static class GuidExtensions
{/*
    public static SerializableGuid ToSerializableGuid(this Guid systemGuid)
    {
        byte[] bytes = systemGuid.ToByteArray();
        return new SerializableGuid(
            BitConverter.ToUInt32(bytes, 0),
            BitConverter.ToUInt32(bytes, 4),
            BitConverter.ToUInt32(bytes, 8),
            BitConverter.ToUInt32(bytes, 12)
        );
    }*/

    public static Guid ToSystemGuid(this SerializableGuid serializableGuid)
    {
        byte[] bytes = new byte[16];
        Buffer.BlockCopy(BitConverter.GetBytes(serializableGuid.Part1), 0, bytes, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(serializableGuid.Part2), 0, bytes, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(serializableGuid.Part3), 0, bytes, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(serializableGuid.Part4), 0, bytes, 12, 4);
        return new Guid(bytes);
    }
}

public static class VisualElementExtensions
{
    public static VisualElement CreateChild(this VisualElement parent, params string[] classes)
    {
        var child = new VisualElement();
        child.AddClass(classes).AddTo(parent);
        return child;
    }

    public static T CreateChild<T>(this VisualElement parent, params string[] classes) where T : VisualElement, new()
    {
        var child = new T();
        child.AddClass(classes).AddTo(parent);
        return child;
    }

    public static T AddTo<T>(this T child, VisualElement parent) where T : VisualElement
    {
        parent.Add(child);
        return child;
    }

    public static T AddClass<T>(this T visualElement, params string[] classes) where T : VisualElement
    {
        foreach (string cls in classes)
        {
            if (!string.IsNullOrEmpty(cls))
            {
                visualElement.AddToClassList(cls);
            }
        }
        return visualElement;
    }

    public static T WithManipulator<T>(this T visualElement, IManipulator manipulator) where T : VisualElement
    {
        visualElement.AddManipulator(manipulator);
        return visualElement;
    }
}

public static class BinaryWriterExtensions
{
    public static void Write(this BinaryWriter writer, SerializableGuid guid)
    {
        writer.Write(guid.Part1);
        writer.Write(guid.Part2);
        writer.Write(guid.Part3);
        writer.Write(guid.Part4);
    }
}

public static class BinaryReaderExtensions
{/*
    public static SerializableGuid Read(this BinaryReader reader)
    {
        return new SerializableGuid(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32());
    }*/
}

public static class UQueryBuilderExtensions
{
    /// <summary>
    /// Sorts the elements of a sequence in ascending order according 
    /// to a key and returns an ordered sequence.
    /// </summary>
    /// <param name="query">The elements to be sorted.</param>
    /// <param name="keySelector">A function to extract a sort key from an element.</param>
    /// <param name="default">The Comparer to compare keys.</param>
    public static IEnumerable<T> OrderBy<T, TKey>(this UQueryBuilder<T> query, Func<T, TKey> keySelector, Comparer<TKey> @default)
        where T : VisualElement
    {
        return query.ToList().OrderBy(keySelector, @default);
    }

    /// <summary>
    /// Sorts the elements of a sequence in ascending order according 
    /// to a numeric key and returns an ordered sequence.
    /// </summary>
    /// <param name="query">The elements to be sorted.</param>
    /// <param name="keySelector">A function to extract a numeric key from an element.</param>
    public static IEnumerable<T> SortByNumericValue<T>(this UQueryBuilder<T> query, Func<T, float> keySelector)
        where T : VisualElement
    {
        return query.OrderBy(keySelector, Comparer<float>.Default);
    }


    /// <summary>
    /// Returns the first element of a sequence, or a default value if no element is found.
    /// </summary>
    /// <param name="query">The elements to search in.</param>
    public static T FirstOrDefault<T>(this UQueryBuilder<T> query)
        where T : VisualElement
    {
        return query.ToList().FirstOrDefault();
    }

    /// <summary>
    /// Counts the number of elements in the sequence that satisfy the condition specified by the predicate function.
    /// </summary>
    /// <param name="query">The sequence of elements to be processed.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    public static int CountWhere<T>(this UQueryBuilder<T> query, Func<T, bool> predicate)
        where T : VisualElement
    {
        return query.ToList().Count(predicate);
    }
}