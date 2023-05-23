using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Sandbox;

namespace Home;
public class NumberLong
{
	[JsonPropertyName("$numberLong")]
	public String RawValue { get; set; }

	public long AsNumber()
	{
		return long.Parse( RawValue );
	}

	public NumberLong()
	{
			
	}

	public NumberLong( long value )
	{
		RawValue = value.ToString();
	}

	public static implicit operator NumberLong( long value )
	{
		return new NumberLong( value );
	}
	public static implicit operator long( NumberLong value )
	{
		return value.AsNumber();
	}
}

public class RemoteDb
{
	private readonly WebSocket _socket;

	public RemoteDb( string uri, string auth )
	{
		Dictionary<string, string> headers = new();
		if ( auth != null )
		{
			headers["Authorization"] = auth;
		}

		_socket = new WebSocket();
		_socket.OnMessageReceived += ResponseReceived;
		Connected = AwaitConnection( uri, headers ).Result == null;
	}

	public bool Connected { get; }

	private async Task<Exception> AwaitConnection(string uri, Dictionary<string, string> headers)
	{
		try
		{
			await _socket.Connect( uri, headers, CancellationToken.None );
			return null;
		}
		catch ( Exception e )
		{
			return e;
		}
	}

	private void ResponseReceived( string message )
	{
		_response = message;
		if(_response == null) _response = "";
	}


	private string _response = null;

	public class ObjectId
	{
		[JsonPropertyName("$oid")]
		public string Oid { get; set; }
	}

	private class QueryResponse<T> where T: HomeData
	{
		[JsonPropertyName("Error")]
		public string Error { get; set; }

		[JsonPropertyName("result")]
		public List<T> Result { get; set; }
	}
	
	public async Task<T> Upsert<T>(T @object) where T: HomeData
	{
		if(!Connected) return null;

		var value = new
		{
			cmd = "update",
			collection = typeof(T).Name,
			@object,
			id = @object.Id?.Oid
		};

		var commandResponse = await SendMessageAndAwaitResponse( value );
		
		var updateResponse = JsonSerializer.Deserialize<T>( commandResponse );

		return updateResponse;
	}
	
	public async Task<List<T>> Query<T>( string query ) where T: HomeData
	{
		if(!Connected) return null;
		
		var value = new
		{
			cmd = "query",
			collection = typeof(T).Name,
			query
		};
		
		var commandResponse = await SendMessageAndAwaitResponse(value);

		var queryResponse = JsonSerializer.Deserialize<QueryResponse<T>>( commandResponse );
		if ( queryResponse.Error != null )
		{
			return await Task.FromException<List<T>>( new Exception( queryResponse.Error ) );
		}

		return queryResponse.Result;
	}

	private async Task<string> SendMessageAndAwaitResponse(object value)
	{
		Log.Info( value );

		var msg = JsonSerializer.Serialize(value);
		_response = null;
		await _socket.Send(msg);
		while (_response == null)
		{
			await Task.Yield();
		}

		var commandResponse = _response;
		_response = null;
		return commandResponse;
	}
}