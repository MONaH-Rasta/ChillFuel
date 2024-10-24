# ChillFuel

Oxide plugin for Rust. Displays the fuel quantity for minicopter, rowboat, rhib, scraptransport and modular car.

When having the permission, players see the specific value of the fuel level of the vehicle and get an alert when the fuel reaches a certain point.
The info can be repositioned by changing the config file.

When having the permission, they can also adjust the settings which are initially taken over from the config values set by the admin.
Values are limited to 200 and can be adjusted by steps of 10.

![](https://i.imgur.com/d2rHV37.png)

## Permissions

* `chillfuel.use` - allows players to be able to see the value
* `chillfuel.modify` - allows players to adjust settings

## Configuration
```
{
  "Postition": {
    "X-axis": 0.285,
    "Y-axis": 0.01
  },
  "Width": 0.045,
  "Minicopter alert": 50,
  "Scrap heli alert": 0,
  "Motorboat alert": 0,
  "RHIB alert": 0,
  "Car alert": 0
}
```

## Chat Commands

* `/fuel`   - displays an interface by which the player can adjust the values

![](https://i.imgur.com/1nNVNcj.pngj)
