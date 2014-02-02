module SimpleGL

open System
open System.Drawing
open System.Collections.Generic
open System.Text
open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL4
open OpenTK.Input
open System.IO

let toRad x = x * (float32) System.Math.PI / 180.0f

let loadshader (name : String) stype = 
    let address = GL.CreateShader(stype)
    use sr = new StreamReader(name)
    GL.ShaderSource(address, sr.ReadToEnd())
    GL.CompileShader(address)
    let r = GL.GetShaderInfoLog(address)
    Console.WriteLine(r)
    address

type Buffer = 
    abstract Bind : unit -> unit

type IBO = {Handle : int;data : int []; usage: BufferUsageHint} with
    static member Create data usage =
        let h = GL.GenBuffer()
        {Handle = h;data = data; usage = usage} 
    static member Bind ibo =
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo.Handle)
        GL.BufferData
            (BufferTarget.ElementArrayBuffer, 
             (nativeint) (ibo.data.Length * sizeof<int>), ibo.data, ibo.usage)
        ()

type Instancing(pos : int, components : int, t : VertexAttribPointerType, norm : bool, size : int, pointer : int) = 
    interface Buffer with
        member this.Bind() = 
            GL.EnableVertexAttribArray(pos)
            GL.VertexAttribPointer(pos, components, t, norm, size, pointer)
            GL.VertexAttribDivisor(pos, 1)

//type VBO<'T when 'T : (new : unit -> 'T) and 'T : struct and 'T :> ValueType> = 
//          {Handle: int
//           target: BufferTarget
//           size: int
//           dataSize: int
//           data: 'T []
//           pos: int
//           usage: BufferUsageHint
//           b: bool} with
//    static member Create target size dataSize pos usage b data =
//        let f = VBO.Create1 target size pos usage b
//        let r = match data with
//                Vec2(v) -> f Vector2.SizeInBytes v
//        r
//    static member Create1 target size pos usage b dataSize data =
//        let h = GL.GenBuffer()
//        {Handle = h
//         target = target
//         size = size
//         dataSize = dataSize
//         data = data
//         pos = pos
//         usage = usage
//         b = b}

//    static member Bind vbo =
//        GL.EnableVertexAttribArray(vbo.pos)
//        GL.BindBuffer(vbo.target, vbo.Handle)
//        GL.BufferData
//            (vbo.target, (nativeint) (vbo.data.Length * vbo.dataSize), vbo.data, vbo.usage)
//        GL.VertexAttribPointer
//            (vbo.pos, vbo.size, VertexAttribPointerType.Float, false, 0, 0)
//        if vbo.b then GL.VertexAttribDivisor(vbo.pos, 1)
//        else GL.VertexAttribDivisor(vbo.pos, 0)
//        ()       
type VBO<'T when 'T : (new : unit -> 'T) and 'T : struct and 'T :> ValueType>(Handle:int, target : BufferTarget, size : int, dataSize : int, data : 'T [], pos : int, usage : BufferUsageHint, b : bool) = 
    interface Buffer with
        member this.Bind() = 
            GL.EnableVertexAttribArray(pos)
            GL.BindBuffer(target, Handle)
            GL.BufferData<'T>
                (target, (nativeint) (data.Length * dataSize), data, usage)
            GL.VertexAttribPointer
                (pos, size, VertexAttribPointerType.Float, false, 0, 0)
            if b then GL.VertexAttribDivisor(pos, 1)
            else GL.VertexAttribDivisor(pos, 0)
            ()
    

type VBO = 
    static member from (target : BufferTarget, size : int,dataSize: int, data,
                        pos : int, usage : BufferUsageHint, b : bool) = 
        let h = GL.GenBuffer()
        VBO(h,target, size, dataSize, data, pos, usage, b)
         
    static member from (target : BufferTarget, size : int, data : float [], 
                        pos : int, usage : BufferUsageHint, b : bool) = 
        VBO.from(target, size, sizeof<float>, data, pos, usage, b)
    static member from (target : BufferTarget, size : int, data : Vector3 [], 
                        pos : int, usage : BufferUsageHint, b : bool) = 
        VBO.from(target, size, Vector3.SizeInBytes, data, pos, usage, b)

type Shader = {Handle: int} with
    static member Create filepath stype =
       let h = loadshader filepath stype
       {Handle = h}

type VertexShader = 
    { shader : Shader }
    static member Create path = 
        { shader = Shader.Create path ShaderType.VertexShader }

type FragmentShader = 
    { shader : Shader }
    static member Create path = 
        { shader = Shader.Create path ShaderType.FragmentShader }

type ShaderProgram = {Handle : int;vs : VertexShader ;fs : FragmentShader}  with
    static member Create (v:VertexShader) f =
        let h = 
            let program = GL.CreateProgram()
            GL.AttachShader(program, v.shader.Handle)
            GL.AttachShader(program, f.shader.Handle)
            GL.LinkProgram(program)
            program
        {Handle = h
         vs=v 
         fs = f}
    static member GetAttribLocation name program =
         GL.GetAttribLocation(program.Handle, name)
    static member GetUniformLocation name program = 
         GL.GetUniformLocation(program.Handle, name)
    static member UniForm4 pos value = GL.Uniform4(pos, ref value)
    static member Uniform1 h (value : float32) = GL.Uniform1(h, value)
    static member UniformMatrix4 h (value : Matrix4) = 
         GL.UniformMatrix4(h, false, ref value)
    static member Use program = GL.UseProgram(program.Handle)
type VAO = { Handle : int } with
    static member Create vboList (ibo: IBO Option) = 
        let h = GL.GenVertexArray()
        GL.BindVertexArray(h)
        vboList |> List.iter (fun (vbo : Buffer) -> vbo.Bind())
        ibo |> Option.iter (fun ibo -> IBO.Bind ibo)
        GL.BindVertexArray(0)
        { Handle = h }
    
    static member Use vao = GL.BindVertexArray(vao.Handle)
    static member UnUse() = GL.BindVertexArray(0)

let indexDraw mode count t vao = 
    VAO.Use vao
    GL.DrawElements(mode, count, t, 0)
    VAO.UnUse()

let draw (ptype, first, count, (vao : VAO)) = 
    VAO.Use vao
    GL.DrawArrays(ptype, first, count)
    VAO.UnUse()

type Mesh = 
    { program : ShaderProgram
      ptype : PrimitiveType
      count : int
      vao : VAO }

let render model mesh = 
    ShaderProgram.UniformMatrix4 (ShaderProgram.GetUniformLocation "model" mesh.program) model
    draw (mesh.ptype, 0, mesh.count, mesh.vao)

type TranslationMatrix4 = Matrix4

type RotationMatrix4 = Matrix4

type Transform = 
    | Translate of TranslationMatrix4
    | Rotation of RotationMatrix4
    | RelRotation of TranslationMatrix4 * RotationMatrix4
    | Id

type Node = 
    { trans : Transform
      mesh : Mesh Option
      nodeList : Node list }

type BoundingBox = 
    { min : Vector3
      max : Vector3 }

type RenderMesh = 
    { model : Matrix4
      mesh : Mesh }

let renderMesh l = l |> List.iter (fun r -> render r.model r.mesh)

let rec genRenderMeshList m graph renderList = 
    let transform = graph.trans
    let mesh = graph.mesh
    let nodeList = graph.nodeList
    
    let model = 
        match transform with
        | Translate(m1) -> m * m1
        | Rotation(m1) -> m1 * m
        | RelRotation(t, r) -> t * r * m
        | Id -> Matrix4.Identity
    
    let f renderMeshList = 
        async { 
            return match nodeList with
                   | [] -> renderMeshList
                   | _ -> 
                       nodeList |> List.fold (fun acc scene -> 
                                       let l = 
                                           genRenderMeshList model scene 
                                               renderMeshList
                                       l @ acc) []
        }
        |> Async.RunSynchronously
    
    match mesh with
    | Some(m) -> 
        let renderMesh = 
            { model = model
              mesh = m }
        f <| renderMesh :: renderList
    | None -> f renderList

let showFps dt = printfn "%f" (1.0f / dt)