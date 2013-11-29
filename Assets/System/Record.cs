using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Record {

    public Table table;
    public string id = string.Empty;
    public List<Field> fields = new List<Field>();

    public Record () {
    }

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
    public Dictionary<string,Record> records = new Dictionary<string,Record>();
}

[System.Serializable]
public class Field {

    public string name = string.Empty;
    public string DebugValue = string.Empty;

    public virtual string Encode () {
        return DebugValue;
    }

    public virtual void Decode ( object obj ) {
        //DebugValue = obj.ToString();
    }

    public Record record;
    public System.Action OnDidSet;

    public Field () {
        OnDidSet = () => DebugValue = Encode();
        //OnDidSet = () => {};
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
            //return value.ToString( "0.0000" );
            return value.ToString();
        }

        public override void Decode ( object obj ) {
            Dynamic.ForValue<float>( obj, value => {
                Set( value );
                //DebugValue = obj.ToString();
            } );
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

        public override void Decode ( object obj ) {
            Dynamic.For<string>( obj, value => {
                Set( value );
            } );
        }
    }
}

class Dynamic {

    public static bool IsNumber ( object value ) {
        return value is sbyte
                || value is byte
                || value is short
                || value is ushort
                || value is int
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is double
                || value is decimal;
    }

    public static void ForValue<T> ( object value, System.Action<T> action ) where T : struct {
        if ( IsNumber( value ) ) {
            action( (T)System.Convert.ChangeType( value, typeof( T ) ) );
        }
    }

    public static void For<T> ( object value, System.Action<T> action ) where T : class {
        T v = value as T;
        if ( v != null ) {
            action( v );
        }
    }
}


