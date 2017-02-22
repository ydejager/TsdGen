namespace TsdGen

module Syntax =
    type Type =
        | Null
        | String
        | Number
        | Union of Type list

    type Member =
        | Property of Name:string * Type:Type

    type Declaration = 
        | Namespace of Name:string * Declarations:Declaration list
        | Interface of Name:string * Members: Member list
