namespace Internal.Importer
{
    [System.Serializable]
    public class FileData
    {
        public SheetData[] m_sheets;
    }

    [System.Serializable]
    public class SheetData
    {
        public string m_name;
        public RowData[] m_rows;
    }

    [System.Serializable]
    public class RowData
    {
        public string m_propertyName;
        public string m_propertyValue;
    }
}