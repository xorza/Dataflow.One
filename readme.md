# Dataflow.One

---
## Background
Photography is my hobby, especially - astrophotography.
Developing an astrophoto image takes a lot of time and usually
is a pretty routine work. Majority of image transformations are the same for every image
and could be automated with some slight adjustments for each image.

I mostly use Photoshop to do that. It is a pain for following reasons:
1. I cannot reuse image processing flow. 'Action recording' doesnt count - it's made of oak.
2. I cannot reuse same layer. E.g: I want it applied twice with different blendmode
or apply one of it's channels as another layer's mask.
3. Poor monetization and product fragmentation.
I need to switch from lightroom to photoshop and back and it always tries to sell me their cloud service.
$50 per month for the whole Creative Suite is not enough for greedy innovationless corporations.

## Solution
Node-based image processing application can solve these pain points
and open new, previously impossible opportunities.

### Tenets:
1. Each operation, such a blending of the images or contrast adjustment - is a node.
2. Nodes can be hierarchical. E.g.: a complex nodegraph can be incapsulated into a node.
3. Each nodegraph is reusable. While processing an image user designs a reusable nodegraph.
4. Nodegraph can be exported without the actual image it was used to process.
Other users can import and reuse it with their images.
5. Each node has a debug preview of its input/output arguments.
6. While processing a nodegraph only nodes which inputs/outputs changed are prcessed for optimization purposes.
7. Software is free and open-source. Developing of a new type of node is trivial.
8. There will be a Node Store for sharing and selling custom nodes.
9. Test-driven. Any functionality should be covered with unit tests. This enforces modularity, testability, etc..

## License

## Contriuting

## 
