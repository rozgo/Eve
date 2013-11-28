using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Record {
    public Table table;
    public string id = string.Empty;
    public List<Field> fields = new List<Field>();

    public T Add<T> ( string name ) where T : Field, new() {
        var field = new T();
        field.name = name;
        field.record = this;
        fields.Add( field );
        return field;
    }
}

[System.Serializable]
public class Table {
    static Dictionary<string, Table> tables = new Dictionary<string, Table>();

    public static Table Get ( string name ) {
        Table table;
        if ( tables.TryGetValue( name, out table ) ) {
            return table;
        }
        table = new Table();
        table.name = name;
        tables[ name ] = table;
        return table;
    }

    public string name = string.Empty;
    public List<string> columns = new List<string>();
    public SortedList<string,Record> records = new SortedList<string,Record>();
}

[System.Serializable]
public class Field {
    public string name = string.Empty;
    public string DebugValue = string.Empty;

    public virtual string Encode () {
        return DebugValue;
    }

    public Record record;
    public System.Action OnDidSet;

    public Field () {
        OnDidSet = () => DebugValue = Encode();
    }

    [System.Serializable]
    public class Number : Field {
        float value = 0;

        public void Set ( float value ) {
            this.value = value;
            OnDidSet();
        }

        public float Get () {
            return value;
        }

        public override string Encode () {
            return value.ToString( "0.0000" );
        }
    }

    [System.Serializable]
    public class String : Field {
        string value = string.Empty;

        public void Set ( string value ) {
            this.value = value;
            OnDidSet();
        }

        public string Get () {
            return value;
        }

        public override string Encode () {
            return value;
        }
    }
}


