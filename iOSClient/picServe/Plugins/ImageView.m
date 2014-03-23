//
//  ImageView.m
//  picServe
//
//  Created by JED LIPPOLD on 8/17/13.
//
//


#import "ImageView.h"
#import "CVHeader.h"
#import "MBProgressHUD.h"
#import "DMActivityInstagram.h"
#import "DMAIDemoViewController.h"
#import "ASIHTTPRequest.h"
#import "MBProgressHUD.h"

@implementation ImageView

@synthesize imageView  = _imageView;
@synthesize toolbar = _toolBar;
@synthesize navBar = _navbar;
@synthesize scrollView = _scrollView;
@synthesize progressHUD = _progressHUD;
@synthesize timer = _timer;


-(CDVPlugin*) initWithWebView:(UIWebView*)theWebView
{
    self = (ImageView*)[super initWithWebView:theWebView];
    [[UIDevice currentDevice] beginGeneratingDeviceOrientationNotifications];
    [[NSNotificationCenter defaultCenter]
     addObserver:self selector:@selector(orientationChanged:)
     name:UIDeviceOrientationDidChangeNotification
     object:[UIDevice currentDevice]];
    
    return self;
}


- (void) orientationChanged:(NSNotification *)note
{
    [self resizeView];
}

- (void)resizeView {
    self.scrollView.zoomScale = 1;
    self.scrollView.contentSize = self.imageView.image.size;
    
    _navbar.frame = CGRectMake(_navbar.frame.origin.x, _navbar.frame.origin.y, self.webView.superview.bounds.size.width, _navbar.frame.size.height);
    _toolBar.frame = CGRectMake(_toolBar.frame.origin.x, self.webView.superview.bounds.size.height - 44, self.webView.superview.bounds.size.width, _toolBar.frame.size.height);
    _imageView.frame = CGRectMake(_imageView.frame.origin.x, _imageView.frame.origin.y, self.webView.superview.bounds.size.width, self.webView.superview.bounds.size.height);
    _scrollView.frame = CGRectMake(_scrollView.frame.origin.x, _scrollView.frame.origin.y, self.webView.superview.bounds.size.width, self.webView.superview.bounds.size.height);
    
}

- (void)createImageView:(NSArray*)arguments withDict:(NSDictionary*)options
{
    
    //nav bar
    CGRect navBarFrame = CGRectMake(0, 0, self.webView.superview.bounds.size.width, 44.0);
    if ([[[UIDevice currentDevice] systemVersion] floatValue] >= 7.0) {
        navBarFrame.size.height = 64;
    }
    _navbar = [[UINavigationBar alloc] initWithFrame:navBarFrame];
    
    //UIImage *backgroundImage = [UIImage imageNamed:@"www/img/navBar.png"];
    //[_navbar setBackgroundImage:backgroundImage forBarMetrics:0];
    
    _navbar.barStyle = UIBarStyleDefault;
    UINavigationItem *navItem = [UINavigationItem alloc];
    NSString *navTitle = @"Image";
    navItem.title = navTitle;
    
    _imageIndex = [[options objectForKey:@"index"] integerValue];

    UIBarButtonItem *leftButton = [[UIBarButtonItem alloc] initWithTitle:@"Close"
                                                                        style:UIBarButtonItemStyleDone
                                                                       target:self
                                                                       action:@selector(hideImageView:) ];
    navItem.leftBarButtonItem = leftButton;
    
    //toolbar
    
    //[[UIToolbar appearance] setBackgroundImage:backgroundImage forToolbarPosition:UIToolbarPositionAny barMetrics:UIBarMetricsDefault];
    
    _toolBar = [[UIToolbar alloc] init];
    
    _toolBar.frame = CGRectMake(0, self.webView.superview.bounds.size.height - 44, self.webView.superview.bounds.size.width, _navbar.frame.size.height);
    [_toolBar sizeToFit];

    UIBarButtonItem *flex = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace
                                                                          target:self
                                                                          action:nil ];
    
    UIBarButtonItem *fit = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemFixedSpace
                                                                          target:self
                                                                          action:nil ];
    fit.width = 15.0f;
    
    UIBarButtonItem *actionButton = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemAction
                                                                                  target:self
                                                                                  action:@selector(shareImage:) ];
    [actionButton  setStyle:UIBarButtonItemStylePlain];
    
    UIBarButtonItem *downloadButton = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemOrganize
                                                                                 target:self
                                                                                 action:@selector(saveImage:) ];
    [downloadButton setStyle:UIBarButtonItemStylePlain];
    /*
    UIButton *info = [UIButton buttonWithType:UIButtonTypeInfoLight];
    [info addTarget:self
                action:@selector(infoTap:)
                forControlEvents:UIControlEventTouchUpInside];


    UIBarButtonItem *infoButton = [[UIBarButtonItem alloc] initWithCustomView:info];
    */
    
    UIBarButtonItem *prevButton = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemRewind
                                                                                    target:self
                                                                                    action:@selector(previousImage:) ];
    [prevButton setStyle:UIBarButtonItemStylePlain];
    
    UIBarButtonItem *nextButton = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemFastForward
                                                                                    target:self
                                                                                    action:@selector(nextImage:) ];
    [nextButton setStyle:UIBarButtonItemStylePlain];
    
    
    NSArray *buttons = [NSArray arrayWithObjects: prevButton, flex, downloadButton, flex, fit, flex, actionButton, flex ,nextButton, nil];
    [_toolBar setItems: buttons animated:NO];
    
    _originalWebViewFrame = self.webView.frame;
	
	CGRect myFrame;
	
	myFrame = CGRectMake(
                         _originalWebViewFrame.origin.x,
                         _originalWebViewFrame.origin.y ,
                         _originalWebViewFrame.size.width,
                         _originalWebViewFrame.size.height
                         );
    
	self.webView.superview.autoresizesSubviews = YES;
    
    CGRect imageViewFrame = CGRectMake(0, 0, self.webView.superview.bounds.size.width, self.webView.superview.bounds.size.height);
    
    
    _scrollView = [[UIScrollView alloc] initWithFrame:imageViewFrame];
    _scrollView.maximumZoomScale = 4.0;
    _scrollView.minimumZoomScale = 1.0;
    _scrollView.delegate = self;
    _scrollView.backgroundColor  = [UIColor whiteColor];
    _scrollView.showsHorizontalScrollIndicator = NO;
    _scrollView.showsVerticalScrollIndicator = NO;
    
    _imageView = [[UIImageView alloc] initWithFrame:imageViewFrame];
    _imageView.contentMode = UIViewContentModeScaleAspectFit;
    _imageView.backgroundColor = [UIColor grayColor];
    //_imageView.image = [UIImage imageNamed:@"logo200.png"];

    [_imageView setUserInteractionEnabled:YES];
    [_imageView setMultipleTouchEnabled:YES];

    
    UISwipeGestureRecognizer *swipeRight = [[UISwipeGestureRecognizer alloc]
                                            initWithTarget:self action:@selector(previousImage:)];
    
    swipeRight.direction = UISwipeGestureRecognizerDirectionRight;
    [_imageView addGestureRecognizer:swipeRight];

    UISwipeGestureRecognizer *swipeLeft = [[UISwipeGestureRecognizer alloc]
                                           initWithTarget:self action:@selector(nextImage:)];
    swipeLeft.direction = UISwipeGestureRecognizerDirectionLeft;
    [_imageView addGestureRecognizer:swipeLeft];

    UITapGestureRecognizer *singleTap = [[UITapGestureRecognizer alloc] initWithTarget:self action:@selector(toggleBars:)];
    [_imageView addGestureRecognizer:singleTap];
    
    UITapGestureRecognizer *doubleTap = [[UITapGestureRecognizer alloc] initWithTarget:self action:@selector(resetZoom:)];
    [doubleTap setNumberOfTapsRequired:2];
    [_imageView addGestureRecognizer:doubleTap];
    
    
    
    [_imageView setHidden:YES];
    
    [_navbar pushNavigationItem:navItem animated:false];
    [_navbar setHidden:YES];
    [_toolBar setHidden:YES];
    [_scrollView setHidden:YES];
    
    [_scrollView addSubview:_imageView];
	[self.webView.superview addSubview:_scrollView];
    [self.webView.superview addSubview:_navbar];
    [self.webView.superview addSubview:_toolBar];
    
}

- (void)setImageViewData:(NSArray*)arguments withDict:(NSDictionary*)options
{
	_imageViewData = [[arguments objectAtIndex:0] copy];
    [self showImage];
}

- (void) showImage {
    _navbar.topItem.title = [NSString stringWithFormat:@"%d of %d", _imageIndex+1, [_imageViewData count]];
    _imageView.image = [UIImage imageNamed:@"logo200.png"];
    _scrollView.zoomScale = 1;
    MBProgressHUD *hud = [MBProgressHUD showHUDAddedTo:self.imageView animated:YES];
    hud.mode = MBProgressHUDModeIndeterminate;
    hud.labelText = @"Loading Hi-Res";
    hud.detailsLabelText = [NSString stringWithFormat:@"Image %d of %d", _imageIndex+1, [_imageViewData count]];
    [hud show:YES];
    NSString *picURL = [[_imageViewData objectAtIndex:_imageIndex] valueForKey:@"url"];
    
    NSString *cachePath = [[_imageViewData objectAtIndex:_imageIndex] valueForKey:@"cachePath"];
    
    //check if the cached version exists
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,  NSUserDomainMask, YES);
    NSString *documentsDirectory = [paths objectAtIndex:0];
    NSString *cachedFile = [documentsDirectory stringByAppendingPathComponent:[NSString stringWithFormat:@"%@.cache", cachePath ]];
    
    BOOL fileExists = [[NSFileManager defaultManager] fileExistsAtPath:cachedFile];
    if (fileExists) {
        NSString *content = [NSString stringWithContentsOfFile:cachedFile encoding:NSUTF8StringEncoding error:nil];
        NSData *imageData = [NSData dataWithContentsOfURL:[NSURL URLWithString:content]];
        _imageView.image  = [UIImage imageWithData:imageData];
    }
    
    //download large image
    NSURL *url = [NSURL URLWithString:picURL];
    __block int currentIndex = _imageIndex;
    __block ASIHTTPRequest *request = [ASIHTTPRequest requestWithURL:url];
    [request setCompletionBlock:^{
        if ([_navbar.topItem.title isEqualToString:[NSString stringWithFormat:@"%d of %d", currentIndex+1, [_imageViewData count]]]) {
            _imageView.image  = [UIImage imageWithData:[request responseData]];
            _imageView.frame = _scrollView.bounds;
            [_imageView setContentMode:UIViewContentModeScaleAspectFit];
            _scrollView.contentSize = CGSizeMake(_imageView.frame.size.width, _imageView.frame.size.height);
        }
        [hud show:NO];
        [hud hide:YES];
        
    }];
    [request setFailedBlock:^{
        [hud show:NO];
        [hud hide:YES];
    }];
    [request startAsynchronous];
    
}

-(void) killQueue {
    
}

- (CGRect)zoomRectForScale:(float)scale withCenter:(CGPoint)center
{
    
    CGRect zoomRect;
    
    // the zoom rect is in the content view's coordinates.
    //    At a zoom scale of 1.0, it would be the size of the imageScrollView's bounds.
    //    As the zoom scale decreases, so more content is visible, the size of the rect grows.
    zoomRect.size.height = [_scrollView frame].size.height / scale;
    zoomRect.size.width  = [_scrollView frame].size.width  / scale;
    
    // choose an origin so as to get the right center.
    zoomRect.origin.x    = center.x - (zoomRect.size.width  / 2.0);
    zoomRect.origin.y    = center.y - (zoomRect.size.height / 2.0);
    
    return zoomRect;
}

- (UIView *)viewForZoomingInScrollView:(UIScrollView *)scrollView
{
    return _imageView;
}

- (IBAction)resetZoom:(id)sender
{
    if (_scrollView.zoomScale == 1) {
        _scrollView.zoomScale = 4;
    } else {
        _scrollView.zoomScale = 1;
    }
}

- (IBAction)shareImage:(id)sender
{
    UIImage *myimage = _imageView.image;
    DMActivityInstagram *instagramActivity = [[DMActivityInstagram alloc] init];
    UIActivityViewController *activityController = [[UIActivityViewController alloc] initWithActivityItems:@[myimage]  applicationActivities:@[instagramActivity]];
    [self.viewController presentViewController:activityController animated:YES completion:nil];
}

-(IBAction)saveImage:(id)sender {
    UIImageWriteToSavedPhotosAlbum(_imageView.image, self, @selector(image:didFinishSavingWithError:contextInfo:), nil);
}

-(IBAction)previousImage:(id)sender {
    
    if (_imageIndex == 0) {
        _imageIndex = [_imageViewData count]-1;
    } else {
        _imageIndex -= 1;
    }
    [self showImage];
}

-(IBAction)nextImage:(id)sender {
    
    if (_imageIndex >= [_imageViewData count]-1 ) {
        _imageIndex = 0;
    } else {
        _imageIndex += 1;
    }

    [self showImage];
}

- (void)image:(UIImage *)image didFinishSavingWithError:(NSError *)error contextInfo:(void *)contextInfo
{
    UIAlertView *alert;
    
    if (error)
        alert = [[UIAlertView alloc] initWithTitle:@"Something went wrong"
                                           message:@"Looks like we have been unable to save this photo in your Camera Roll. Just try again."
                                          delegate:self cancelButtonTitle:@"Ok"
                                 otherButtonTitles:nil];
    else
        alert = [[UIAlertView alloc] initWithTitle:@"Success!"
                                           message:@"Photo saved to your Camera Roll."
                                          delegate:self cancelButtonTitle:@"Ok"
                                 otherButtonTitles:nil];
    [alert show];
}

- (void)showImageView:(NSArray*)arguments withDict:(NSDictionary*)options
{
	if(nil == _imageView){
        [self createImageView:nil withDict:nil];
	}
	
    
	if(NO == [_imageView isHidden]){
		return;
	}
    [self resizeView];
	_originalWebViewFrame = self.webView.frame;
    
    [self fadeIn];
    
}

- (void)playVideo:(NSArray*)arguments withDict:(NSDictionary*)options
{
    
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,  NSUserDomainMask, YES);
    NSString *documentsDirectory = [paths objectAtIndex:0];
    NSString *cachedFile = [documentsDirectory stringByAppendingPathComponent:[options objectForKey:@"title"]];
    NSLog(@"%@", cachedFile);
    BOOL fileExists = [[NSFileManager defaultManager] fileExistsAtPath:cachedFile];

    if (fileExists) {
        [self downloadVideo:cachedFile];
        NSString * jsCallBack = [NSString stringWithFormat:@"window.plugins.ImageView._onVideoDownloaded('%@');", cachedFile];
        [self.webView stringByEvaluatingJavaScriptFromString:jsCallBack];
    } else {

        self.progressHUD = nil;
        self.progressHUD = [MBProgressHUD showHUDAddedTo:self.webView.superview animated:YES];
        self.progressHUD.mode = MBProgressHUDModeAnnularDeterminate;
        self.progressHUD.labelText = @"Downloading Video";
        self.progressHUD.detailsLabelText = @"Tap to cancel.";
        [self.progressHUD addGestureRecognizer:[[UITapGestureRecognizer alloc] initWithTarget:self action:@selector(hudWasTapped)]];
        
        NSString *url = [options objectForKey:@"video"];
        __block ASIHTTPRequest *request = [ASIHTTPRequest requestWithURL:[NSURL URLWithString:url]];
        [request setDownloadProgressDelegate:self];
        [request setShowAccurateProgress: YES];
        
        self.timer = nil;
        _timer = nil;
        if (self.timer == nil) {
            self.timer = [NSTimer scheduledTimerWithTimeInterval:1.0f/60.0f target:self selector:@selector(setProgress:) userInfo:nil repeats:YES];
        }

        [request setCompletionBlock:^{
            NSData *videoData = [request responseData];
            [videoData writeToURL:[NSURL fileURLWithPath:cachedFile] options:0 error:nil];
            [self.progressHUD hide:YES];
            
            [self downloadVideo:cachedFile];
            
            NSString * jsCallBack = [NSString stringWithFormat:@"window.plugins.ImageView._onVideoDownloaded('%@');", cachedFile];
            [self.webView stringByEvaluatingJavaScriptFromString:jsCallBack];
        }];
        [request setFailedBlock:^{
            [self.progressHUD hide:YES];
        }];
        [request startAsynchronous];
        
        
    }
}

-(void)downloadVideo:(NSString *)sampleMoviePath{
    if (UIVideoAtPathIsCompatibleWithSavedPhotosAlbum(sampleMoviePath)){
        UISaveVideoAtPathToSavedPhotosAlbum(sampleMoviePath, self, @selector(video:didFinishSavingWithError: contextInfo:), nil);
    }
}

-(void)video:(NSString *)videoPath didFinishSavingWithError:(NSError *)error contextInfo:(void *)contextInfo{
   // NSLog(@"Finished with error: %@", error);
}

- (void)hudWasTapped {

    
    for (ASIHTTPRequest *req in ASIHTTPRequest.sharedQueue.operations)
    {
        [req cancel];
        [req setDelegate:nil];
    }
    
    
    if (self.timer != nil) {
        [self.timer invalidate];
        self.timer = nil;
    }

    [self.progressHUD hide:YES];
    
}


- (void)setProgress:(float)progress
{
    NSLog(@"currProg: %f", progress);
    if (progress < 1.0) {
        if (progress > 0.01) {
            self.progressHUD.progress = progress;
        }
    } else {
        
        if (self.timer != nil) {
            [self.timer invalidate];
            self.timer = nil;
        }
    }
}

- (IBAction)hideImageView:(id)sender
{

	if(nil == _imageView){
        return;
	}
	
	if(YES == [_imageView isHidden]){
		return;
	}
    
    [self fadeOut];
	
	[self.webView setFrame:_originalWebViewFrame];
    
}

-(void)fadeIn
{
    CGRect r = [_imageView frame];
    r.origin.y = [_imageView frame].size.height;
    [_imageView setFrame:r];
    _imageView.alpha = 0;
    _navbar.alpha = 0;
    _toolBar.alpha = 0;
    _scrollView.alpha = 0;
    [_imageView setHidden:NO];
    [_navbar setHidden:NO];
    [_toolBar setHidden:NO];
    [_scrollView setHidden:NO];

    [UIView animateWithDuration:0.3
                          delay: 0.0
                        options:UIViewAnimationCurveEaseInOut
                     animations:^{
                         CGRect r = [_imageView frame];
                         r.origin.y = 0;
                         [_imageView setFrame:r];
                         _imageView.alpha = 1;
                         _navbar.alpha = 1;
                         _toolBar.alpha = 1;
                         _scrollView.alpha = 1;
                     }
                     completion:^(BOOL finished){
                     }];
    
}

-(void)fadeOut
{
    
    
    CGRect r = [_imageView frame];
    r.origin.y = 0;
    [_imageView setFrame:r];
    _imageView.alpha = 1;
    _navbar.alpha = 1;
    _toolBar.alpha = 1;
    _scrollView.alpha = 1;
    [_navbar setHidden:NO];
    [_toolBar setHidden:NO];
    [_imageView setHidden:NO];
    [_scrollView setHidden:NO];
    
    [UIView animateWithDuration:0.3
                          delay: 0.0
                        options:UIViewAnimationCurveEaseInOut
                     animations:^{
                         CGRect r = [_imageView frame];
                         r.origin.y = [_imageView frame].size.height;
                         [_imageView setFrame:r];
                         _imageView.alpha = 0;
                         _navbar.alpha = 0;
                         _scrollView.alpha = 0;
                         _toolBar.alpha = 0;
                     }
                     completion:^(BOOL finished){
                         [_navbar setHidden:YES];
                         [_imageView setHidden:YES];
                         [_scrollView setHidden:YES];
                        [_toolBar setHidden:YES];
                     }];
    
    
}

- (IBAction)toggleBars:(id)sender
{
    if([_navbar isHidden] == NO){
        _navbar.alpha = 1;
        _toolBar.alpha = 1;
        [_navbar setHidden:NO];
        [_toolBar setHidden:NO];
        
        [UIView animateWithDuration:0.3
                              delay: 0.0
                            options:UIViewAnimationCurveEaseInOut
                         animations:^{
                             _navbar.alpha = 0;
                             _toolBar.alpha = 0;
                         }
                         completion:^(BOOL finished){
                             [[UIApplication sharedApplication] setStatusBarHidden:YES withAnimation:UIStatusBarAnimationFade];
                             [_navbar setHidden:YES];
                             [_toolBar setHidden:YES];
                         }];
        
        
    } else {

        _navbar.alpha = 0;
        _toolBar.alpha = 0;
        [_navbar setHidden:YES];
        [_toolBar setHidden:YES];
        
        [UIView animateWithDuration:0.3
                              delay: 0.0
                            options:UIViewAnimationCurveEaseInOut
                         animations:^{
                             _navbar.alpha = 1;
                             _toolBar.alpha = 1;
                         }
                         completion:^(BOOL finished){
                            [[UIApplication sharedApplication] setStatusBarHidden:NO withAnimation:UIStatusBarAnimationFade];
                             [_navbar setHidden:NO];
                             [_toolBar setHidden:NO];

                         }];
        
        
    }

    
}
@end
