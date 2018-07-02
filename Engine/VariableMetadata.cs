namespace Evolutionary
{
    class VariableMetadata<T>
    {
        // Each VariableNode has a pointer to this object, to retrieve the value
        public T Value { get; set; }
        public string Name { get; set; }
    }
}
