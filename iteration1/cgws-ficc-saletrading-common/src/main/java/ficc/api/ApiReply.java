package ficc.api;

/** FICC 应答信封；Data 的具体类型由 Endpoint 指定。 */
public class ApiReply<T extends ProtocolModel> extends Reply {
    public T Data;
}
