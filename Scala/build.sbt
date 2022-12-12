val scala3Version = "3.2.1"

lazy val root = project
  .in(file("."))
  .settings(
    name := "day1",
    version := "0.1.0-SNAPSHOT",

    scalaVersion := scala3Version,

    scalacOptions += "-Werror",
    scalacOptions += "-deprecation",

    libraryDependencies += "org.scalatest" %% "scalatest" % "3.2.14"
  )
