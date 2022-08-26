# ywh-HFS

局域网中实现两台PC之间的文件传输


# 预想

参考[HFS](https://www.rejetto.com/hfs/?f=intro)软件，做一个简易版的，顺便学习C#

虽然不清楚原版是如何实现的，在我想法中，应该是开启一个http服务之类的，客户端可以通过网页访问，有一个页面什么的

第一步：先搭建起http服务，并且能通过网页访问：2022/8/26完成


# HTTP

[HTTP参考链接](https://www.runoob.com/http/http-intro.html)

HTTP 协议一般指 HTTP（超文本传输协议）。

超文本传输协议（英语：HyperText Transfer Protocol，缩写：HTTP）是一种用于分布式、协作式和超媒体信息系统的应用层协议，是因特网上应用最为广泛的一种网络传输协议，所有的 WWW 文件都必须遵守这个标准。

HTTP 是为 Web 浏览器与 Web 服务器之间的通信而设计的，但也可以用于其他目的。

HTTP 是一个基于 TCP/IP 通信协议来传递数据的（HTML 文件、图片文件、查询结果等）。

HTTP 三点注意事项：

HTTP 是无连接：无连接的含义是限制每次连接只处理一个请求，服务器处理完客户的请求，并收到客户的应答后，即断开连接，采用这种方式可以节省传输时间。

HTTP 是媒体独立的：这意味着，只要客户端和服务器知道如何处理的数据内容，任何类型的数据都可以通过HTTP发送，客户端以及服务器指定使用适合的 MIME-type 内容类型。

HTTP 是无状态：HTTP 协议是无状态协议，无状态是指协议对于事务处理没有记忆能力，缺少状态意味着如果后续处理需要前面的信息，则它必须重传，这样可能导致每次连接传送的数据量增大，另一方面，在服务器不需要先前信息时它的应答就较快。


HTTP无状态协议的意思用白话说：

- 有状态：

	A：你今天中午吃的啥？

	B：吃的大盘鸡。

	A：味道怎么样呀？

	B：还不错，挺好吃的。

- 无状态：

	A：你今天中午吃的啥？

	B：吃的大盘鸡。

	A：味道怎么样呀？

	B：？？？啊？啥？啥味道怎么样？

无状态对上下文是没有记忆点的

[HTTP无状态更多讨论](https://www.zhihu.com/question/23202402)


# Socket和多线程、异步


为了防止主线程在等待客户端连接，不能执行其他任务，开启一个线程来执行等待这个操作

```C#

Thread thread = new Thread(() => { Listener(); });
thread.Start()

private void Listener()
{

    while (true)
    {
        // 等待客户端连接
        TcpClient client = serverListener.AcceptTcpClient();
    }

}

```

只是用线程来承接每一次的客户端连接会产生以下问题

	a、 每个socket请求，建立一个连接，当每个都是进行简短的通信时，则异常的耗费系统建立、销毁线程资源。

	b、 如果建立线程太多，每个线程都会占用一定的系统内存，这样将导致内存溢出。

	c、 频繁地对线程进行建立 销毁，会导致操作系统进行频繁的cpu切换线程切换，这样也会非常耗费系统资源。

## 线程池

[浅谈线程池（上）：线程池的作用及CLR线程池 ](https://www.cnblogs.com/JeffreyZhao/archive/2009/07/22/thread-pool-1-the-goal-and-the-clr-thread-pool.html)

因为频繁的对线程操作，会导致一系列问题，对上面的优化就是使用**线程池**来处理Socket

个人理解：线程池就是保存了很多的线程的一种容器，我们有需要就去拿，用完了就放回去，不用再频繁的创建和销毁了

## 异步编程

[C#关于异步的讨论](https://www.zhihu.com/question/56651792/answer/149968434)

以下是摘抄上面链接中一段回答
> await和async隐藏了很多复杂的细节，不了解的话你就很难正确的理解await和async。
> 一般我们需要异步的地方都是在进行比较耗时的操作，比如说磁盘IO、网络IO，当你以同步的方式调用系统API进行磁盘读取或者获取网络数据的时候，
> 你的线程会阻塞在那里等待什么事也干不了，直到操作系统从底层返回IO数据。这就是为什么会有异步模式的存在。
> 异步模式就是说在执行耗时IO API的时候线程不等待结果而是直接返回并注册一个回调函数，当操作系统完成耗时操作的时候，
> 调用回调函数通知你IO结果。await和async就是为了方便我们调用异步API而生的。当你await一个异步API的时候，你的await语句就是当前函数的最后一条语句，
> 你肯定觉得我在胡说，明明很多时候await语句后面还有代码，这是因为编译器在后面做了很多工作。
> 每个异步API都返回的是一个Task对象，当你await异步API的时候你就能获得这个Task对象,这个Task对象所代表的就是我上面说的那个异步模式的回调函数。
> Task对象有个函数叫ContinueWith，他接受的参数是一个delegate（delegate代表的就是某一个函数），
> ContinueWith的意思就是说当前Task对象代表的函数执行完后，继续执行ContinueWith注册进去的delegate。
> 编译器就是将await语句后面的代码抽出来变成了另外一个函数，并用ContinueWith注册进await返回的那个Task对象。
> 所以总的流程就是，当你await一个异步API的时候，返回一个代表第二段所说的回调函数的Task对象，并将await之后的代码注册进Task对象，
> 当前函数就执行完了立即返回，这个时候底层操作系统还在帮你进行费时的IO操作还没拿到结果，但你的函数已经返回上层调用了。
> 当操作系统完成IO后，他就会回调那个Task对象，于是你的await指令后面的代码也就随之执行了。
> 你仔细观察就会发现你执行await时候的线程ID和await之后代码的线程ID是不一样的，说明是两个不同的线程执行的代码，await之后代码是用一种叫做IO线程来执行的， await之前的线程叫做worker线程。



## Socket异常

- System.Net.Sockets.SocketException:“一个封锁操作被对 WSACancelBlockingCall 的调用中断

因为**TcpListener.AcceptTcpClient()**是**阻塞式**等待客户端连接，直到接到一个**TcpClient**，或者出错才会进行下一步，此时我们调用**TcpListener.Stop()**必然会出错

所以需要使用try{}catch{}来捕获这个异常就可以了

# 异步编程

[C# 中的Async 和 Await 的用法详解](https://www.cnblogs.com/yilezhu/p/10555849.html)

async： 使一个方法变为异步，不需要执行完成就可以执行下一步

await： 对于一个异步方法，下一步的操作可能依赖异步方法完成后的结果，需要对异步方法添加await关键字