using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using OpenAI.Chat;

namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;

public class SignalParser
{
    private enum ParserState : byte
    {
        InTag,
        InContent,
        InName
    }
    private ParserState _state = ParserState.InContent;
    private ContentType _contentType = ContentType.Name;
    
    private readonly StringBuilder _valueAccum = new();
    private readonly StringBuilder _signalAccum =  new();
    private readonly StringBuilder _nameAccum =  new();
    private StringBuilder? _writeBuffer;
    
    private int _writeBufferIndex;
    private const string SigTagOpenPrefix = "<sig type=\"";
    private const string SigTagClose = "</sig>";
    
    private void SwapBuffers()
    {
        _writeBuffer = _writeBuffer == _signalAccum ? _valueAccum : _signalAccum;
    }
    
    private void Consume(char ch)
    {
        if (_writeBuffer == null) return;
            
        if (_state == ParserState.InTag)
        {
            _writeBuffer.Append(ch);
            
            var validOpen = !(_writeBufferIndex < SigTagClose.Length && SigTagClose[_writeBufferIndex] != ch);
            var validClose = !(_writeBufferIndex < SigTagOpenPrefix.Length && SigTagOpenPrefix[_writeBufferIndex] != ch);

            if (!validOpen && !validClose)
            {
                var currentContent = _writeBuffer.ToString();
                _writeBuffer.Clear();
                SwapBuffers();
                _writeBuffer.Append(currentContent);
                _state = ParserState.InContent;
                _writeBufferIndex = 0;
                return;
            }

            if (validClose && _writeBufferIndex == SigTagOpenPrefix.Length - 1) 
            {
                _state = ParserState.InName;
            }
        }else if (_state == ParserState.InName)
        {
            if (ch == '"')
            {
                _contentType = _nameAccum.ToString() switch
                {
                    "name" =>  ContentType.Name,
                    "code" =>  ContentType.Code,
                    _ => ContentType.Text
                };
                _nameAccum.Clear();
                _state = ParserState.InTag;
            }
            else
            {
                _nameAccum.Append(ch);
            }
        }
        else
        {
            _writeBuffer.Append(ch);
        }
        _writeBufferIndex++;
    }
    
    public async IAsyncEnumerable<ChatCompletionStreamedDto> Parse(IAsyncEnumerable<SimpleStreamingUpdate> completionStream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _writeBuffer = _valueAccum;
        await foreach (var completionUpdate in completionStream.WithCancellation(cancellationToken))
        {
            foreach (var t in completionUpdate.ContentUpdate[0].Text)
            {
                switch (t)
                {
                    case '<':
                        if (_state == ParserState.InContent)
                        {
                            _writeBuffer.Clear();
                            _writeBufferIndex = 0;
                            SwapBuffers();
                            _state = ParserState.InTag;
                        }
                        Consume(t);
                        break;
                    case '>':
                        Consume(t);
                        if (_state == ParserState.InTag)
                        {
                            _state = ParserState.InContent;
                            _writeBuffer.Clear();
                            _writeBufferIndex = 0;
                            SwapBuffers();
                        }
                        break;
                    default:
                        Consume(t);
                        break;
                }
            }
            if (_valueAccum.Length == 0)
            {
                continue;
            }
            
            yield return new ChatCompletionStreamedDto
            {
                Message = _valueAccum.ToString(),
                Type = _contentType,
            };
            _valueAccum.Clear();
        }
    }
    
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContentType
{
    Name, Code, Text
}