module CubeData

open OpenTK
let topLeftFront = Vector3(-1.0f,1.0f,-1.0f)
let topRightFront = Vector3(1.0f,1.0f,-1.0f)
let botRightFront = Vector3(1.0f,-1.0f,-1.0f)
let botLeftFront = Vector3(-1.0f,-1.0f,-1.0f)

let topLeftBack = Vector3(-1.0f,1.0f,1.0f)
let topRightBack = Vector3(1.0f,1.0f,1.0f)
let botRightBack = Vector3(1.0f,-1.0f,1.0f)
let botLeftBack = Vector3(-1.0f,-1.0f,1.0f)

let nRight = Vector3(1.0f,0.0f,0.0f)
let nLeft = Vector3(-1.0f,0.0f,0.0f)
let nUp = Vector3(0.0f,1.0f,0.0f)
let nDown = Vector3(0.0f,-1.0f,0.0f)
let nFront = Vector3(0.0f,0.0f,-1.0f)
let nBack = Vector3(0.0f,0.0f,1.0f)

let ndata =
    [|nFront
      nFront
      nFront
      nFront

      nBack
      nBack
      nBack
      nBack

      nRight
      nRight
      nRight
      nRight

      nLeft
      nLeft
      nLeft
      nLeft
      
      nUp
      nUp
      nUp
      nUp

      nDown
      nDown
      nDown
      nDown|]
let vertdata = 
    [| 
      topLeftFront
      botLeftFront
      botRightFront
      topRightFront

      topRightBack
      botRightBack
      botLeftBack
      topLeftBack

      topRightFront
      botRightFront
      botRightBack
      topRightBack

      topLeftFront
      topLeftBack
      botLeftBack
      botLeftFront

      topLeftFront
      topRightFront
      topRightBack
      topLeftBack

      botLeftFront
      botRightFront
      botRightBack
      botLeftBack |]
