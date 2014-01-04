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

let loadshader (name : String) stype = 
    let address = GL.CreateShader(stype)
    use sr = new StreamReader(name)
    GL.ShaderSource(address, sr.ReadToEnd())
    GL.CompileShader(address)
    address

type VBO(target : BufferTarget, size : int, data : Vector3 [], pos : int, usage : BufferUsageHint) = 
    let h = GL.GenBuffer()
    member this.Pos = pos
    
    member this.Bind() = 
        GL.BindBuffer(target, h)
        GL.BufferData
            (target, (nativeint) (data.Length * Vector3.SizeInBytes), data, 
             usage)
        GL.VertexAttribPointer
            (pos, size, VertexAttribPointerType.Float, false, 0, 0)
        ()
    
    member this.Handle = h

type Shader(filepath : String, stype : ShaderType) = 
    member this.Handle = loadshader filepath stype

type ShaderProgram(shaders) = 
    member this.GetAttribLocation(name : String) = 
        GL.GetAttribLocation(this.Handle, name)
    
    member this.Handle : int = 
        let program = GL.CreateProgram()
        shaders |> List.iter (fun (s : Shader) -> 
                       GL.AttachShader(program, s.Handle)
                       ())
        GL.LinkProgram(program)
        program
    
    member this.Use() = GL.UseProgram(this.Handle)

type VAO(buffers) = 
    
    let handle = 
        let h = GL.GenVertexArray()
        GL.BindVertexArray(h)
        buffers |> List.iter (fun (vbo : VBO) -> 
                       GL.EnableVertexAttribArray(vbo.Pos)
                       vbo.Bind())
        GL.BindVertexArray(0)
        h
    
    member this.Handle = handle
    member this.UnUse() = GL.BindVertexArray(0)
    member this.Use() = GL.BindVertexArray(this.Handle)

let draw ptype dstart dend (vao : VAO) = 
    vao.Use()
    GL.DrawArrays(ptype, dstart, dend)
    vao.UnUse()