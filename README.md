# EntitySelection
A small package that enables you to select entities in the unity sceneview.  
Works with every RenderPipeline.

## How to select
Make sure your focus is on the sceneview (not gameview)  
Point to the entity with mouse pointer  
Press the '1' key (Non Numpad)  
=> Inspector window should show all info for entity.  

## How to get this package for your own project
1. Click the green "Clone or download" button and copy the url.  
2. In Unity go to Window>Package Manager and Press the + sign in the left-top corner of the Package Manager window.  
3. Select "Add package from git URl...", Paste the URL and press "Add".  
Done!  

## Contribution
BugFixes and UX improvements are appreciated.  
Performance Improvements are welcome, since i'm not prioritizing those.  
No setup should ever be needed.  

Thanks to [Tom](https://github.com/Moosichu) for making this repo a package.  

## Todo
* Visual selection feedback (Blocked, Adding CMD Buffers to SceneView Camera seems to be ignored.)
* Use left-click for selection (Blocked, OnMouseUp event currently gets consumed by the SceneView, not sure a workaround exists)  
* Low Priority: Performance Improvements (Tested with 20.000 entities, no issues)
