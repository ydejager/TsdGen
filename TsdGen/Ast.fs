namespace TsdGen

module Syntax =
    type Id = | Id of string
    type PropName = | PropName of string
    type Ns = Id list
    type TypeName = Id

    type Type =
        | Null
        | String
        | Number
        | Bool
        | List of Type
        | Object of (Ns * TypeName)
        | Union of Type list
        | Generic of TypeName * Type list

    type Member =
        | Property of PropName * Type:Type

    type Extends = | Extends of Ns * TypeName

    type Declaration = 
        | Namespace of Ns * Declarations:Declaration list
        | Interface of TypeName * Extends option * Members: Member list
