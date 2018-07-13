using System.Text;

namespace Hessian.Util
{

	/// <summary>
	/// The IntMap provides a simple hashmap from keys to integers.  The API is
	/// an abbreviation of the HashMap collection API.
	/// 
	/// <para>The convenience of IntMap is avoiding all the silly wrapping of
	/// integers.
	/// </para>
	/// </summary>
	public class IntMap
	{
	  /// <summary>
	  /// Encoding of a null entry.  Since NULL is equal to Integer.MIN_VALUE, 
	  /// it's impossible to distinguish between the two.
	  /// </summary>
	  public const int NULL = unchecked((int)0xdeadbeef); // Integer.MIN_VALUE + 1;

	  private static readonly object DELETED = new object();

	  private object[] _keys;
	  private int[] _values;

	  private int _size;
	  private int _mask;

	  /// <summary>
	  /// Create a new IntMap.  Default size is 16.
	  /// </summary>
	  public IntMap()
	  {
		_keys = new object[256];
		_values = new int[256];

		_mask = _keys.Length - 1;
		_size = 0;
	  }

	  /// <summary>
	  /// Clear the hashmap.
	  /// </summary>
	  public virtual void clear()
	  {
		object[] keys = _keys;
		int[] values = _values;

		for (int i = keys.Length - 1; i >= 0; i--)
		{
		  keys[i] = null;
		  values[i] = 0;
		}

		_size = 0;
	  }
	  /// <summary>
	  /// Returns the current number of entries in the map.
	  /// </summary>
	  public virtual int size()
	  {
		return _size;
	  }

	  /// <summary>
	  /// Puts a new value in the property table with the appropriate flags
	  /// </summary>
	  public virtual int get(object key)
	  {
		int mask = _mask;
		int hash = key.GetHashCode() % mask & mask;

		object[] keys = _keys;

		while (true)
		{
		  object mapKey = keys[hash];

		  if (mapKey == null)
		  {
			return NULL;
		  }
		  else if (mapKey == key || mapKey.Equals(key))
		  {
			return _values[hash];
		  }

		  hash = (hash + 1) % mask;
		}
	  }

	  /// <summary>
	  /// Expands the property table
	  /// </summary>
	  private void resize(int newSize)
	  {
		object[] newKeys = new object[newSize];
		int[] newValues = new int[newSize];

		int mask = _mask = newKeys.Length - 1;

		object[] keys = _keys;
		int[] values = _values;

		for (int i = keys.Length - 1; i >= 0; i--)
		{
		  object key = keys[i];

		  if (key == null || key == DELETED)
		  {
			continue;
		  }

		  int hash = key.GetHashCode() % mask & mask;

		  while (true)
		  {
			if (newKeys[hash] == null)
			{
			  newKeys[hash] = key;
			  newValues[hash] = values[i];
			  break;
			}

			hash = (hash + 1) % mask;
		  }
		}

		_keys = newKeys;
		_values = newValues;
	  }

	  /// <summary>
	  /// Puts a new value in the property table with the appropriate flags
	  /// </summary>
	  public virtual int put(object key, int value)
	  {
		int mask = _mask;
		int hash = key.GetHashCode() % mask & mask;

		object[] keys = _keys;

		while (true)
		{
		  object testKey = keys[hash];

		  if (testKey == null || testKey == DELETED)
		  {
			keys[hash] = key;
			_values[hash] = value;

			_size++;

			if (keys.Length <= 4 * _size)
			{
			  resize(4 * keys.Length);
			}

			return NULL;
		  }
		  else if (key != testKey && !key.Equals(testKey))
		  {
			hash = (hash + 1) % mask;

			continue;
		  }
		  else
		  {
			int old = _values[hash];

			_values[hash] = value;

			return old;
		  }
		}
	  }

	  /// <summary>
	  /// Deletes the entry.  Returns true if successful.
	  /// </summary>
	  public virtual int remove(object key)
	  {
		int mask = _mask;
		int hash = key.GetHashCode() % mask & mask;

		while (true)
		{
		  object mapKey = _keys[hash];

		  if (mapKey == null)
		  {
			return NULL;
		  }
		  else if (mapKey == key)
		  {
			_keys[hash] = DELETED;

			_size--;

			return _values[hash];
		  }

		  hash = (hash + 1) % mask;
		}
	  }

	  public override string ToString()
	  {
		StringBuilder sbuf = new StringBuilder();

		sbuf.Append("IntMap[");
		bool isFirst = true;

		for (int i = 0; i <= _mask; i++)
		{
		  if (_keys[i] != null && _keys[i] != DELETED)
		  {
			if (!isFirst)
			{
			  sbuf.Append(", ");
			}

			isFirst = false;
			sbuf.Append(_keys[i]);
			sbuf.Append(":");
			sbuf.Append(_values[i]);
		  }
		}
		sbuf.Append("]");

		return sbuf.ToString();
	  }
	}

}